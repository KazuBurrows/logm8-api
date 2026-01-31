using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;

public class NfcTagRepository : INfcTagRepository
{
    private readonly CosmosConnectionFactory _factory;
    private readonly SqlConnectionFactory _sqlFactory;

    public NfcTagRepository(CosmosConnectionFactory factory, SqlConnectionFactory sqlFactory)
    {
        _factory = factory;
        _sqlFactory = sqlFactory;
    }

    public async Task<string?> GetNfcTagIdByUriTokenAsync(string uriToken)
    {
        const string query =
            "SELECT TOP 1 [LogId] FROM [dbo].[onelifes] WHERE [TokenKey] = @TokenKey";

        using var conn = _sqlFactory.Create();
        await conn.OpenAsync();

        using var command = new SqlCommand(query, conn);
        command.Parameters.AddWithValue("@TokenKey", uriToken);

        var result = await command.ExecuteScalarAsync();
        return result as string;
    }

    public async Task<int> ReplaceAllRecordsForTagAsync(string oldTagId, string newTagId)
    {
        var container = _factory.GetContainer(CosmosContainer.Records.GetName());

        var query = new QueryDefinition("SELECT * FROM c WHERE c.TagId = @oldTagId").WithParameter(
            "@oldTagId",
            oldTagId
        );

        using var iterator = container.GetItemQueryIterator<Record>(
            query,
            requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(oldTagId) }
        );

        int migratedCount = 0;

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();

            foreach (var record in page)
            {
                var oldId = record.id;

                // ---- mutate record for new partition ----
                record.id = Guid.NewGuid().ToString();
                record.TagId = newTagId;

                // Optional audit fields
                record.Comment += $" (Migrated from TagId {oldTagId} at {DateTime.UtcNow})";

                // ---- create new record in new partition ----
                await container.CreateItemAsync(record, new PartitionKey(newTagId));

                // ---- delete old record ----
                await container.DeleteItemAsync<Record>(oldId, new PartitionKey(oldTagId));

                migratedCount++;
            }
        }

        return migratedCount;
    }

    public async Task<bool> UpdateNfcTagAsync(Tag tag)
    {
        var container = _factory.GetContainer(CosmosContainer.Tags.GetName());

        try
        {
            // Fix TagId format. '+' lost when in url and is decoded to ' '.
            string tagId = tag.TagId.Replace(" ", "+");
            tag.TagId = tagId;

            // Create a query to retrieve the item using TagId as the partition key
            var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.TagId = @tagId"
            ).WithParameter("@tagId", tagId);

            var iterator = container.GetItemQueryIterator<Tag>(queryDefinition);

            // Execute the query
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var item in response)
                {
                    // Modify the fields you want to update
                    item.Make = tag.Make;
                    item.Model = tag.Model;
                    item.Year = tag.Year;
                    item.Vehicle = tag.Vehicle;
                    item.Style = tag.Style;
                    item.Engine = tag.Engine;
                    item.Fuel = tag.Fuel;
                    item.Transmission = tag.Transmission;
                    item.Color = tag.Color;
                    item.VinNumber = tag.VinNumber;
                    item.LicencePlate = tag.LicencePlate;
                    item.IsConfigured = true;

                    // Replace the item in Cosmos DB
                    await container.ReplaceItemAsync(item, item.id, new PartitionKey(item.TagId));

                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}

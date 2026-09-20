using LogMate.Domain.Enums;
using LogMate.Domain.Models;
using LogMate.Infrastructure.Data.Interfaces;
using LogMate.Infrastructure.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;

namespace LogMate.Infrastructure.Data.Repositories;

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

    public async Task<OneLifeTokenInfo?> GetOneLifeTokenAsync(string token)
    {
        const string query =
            "SELECT TOP 1 [LogId], [UserId], [Mode], [TTL] FROM [dbo].[onelifes] WHERE [TokenKey] = @TokenKey";

        using var conn = _sqlFactory.Create();
        await conn.OpenAsync();

        using var command = new SqlCommand(query, conn);
        command.Parameters.AddWithValue("@TokenKey", token);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        string? logId = reader.IsDBNull(reader.GetOrdinal("LogId")) ? null : reader.GetString(reader.GetOrdinal("LogId"));
        string? userId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetString(reader.GetOrdinal("UserId"));
        int? mode = reader.IsDBNull(reader.GetOrdinal("Mode")) ? null : reader.GetInt32(reader.GetOrdinal("Mode"));
        DateTime? ttl = reader.IsDBNull(reader.GetOrdinal("TTL")) ? null : reader.GetDateTime(reader.GetOrdinal("TTL"));

        if (string.IsNullOrEmpty(logId) || !ttl.HasValue || ttl.Value < DateTime.UtcNow)
            return null;

        return new OneLifeTokenInfo(logId, userId, mode, ttl.Value);
    }

    public async Task<int> ReplaceAllRecordsForTagAsync(string oldTagId, string newTagId)
    {
        var container = _factory.GetContainer(CosmosContainer.Records.GetName());

        var query = new QueryDefinition("SELECT * FROM c WHERE c.TagId = @oldTagId")
            .WithParameter("@oldTagId", oldTagId);

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

                record.id = Guid.NewGuid().ToString();
                record.TagId = newTagId;
                record.Comment += $" (Migrated from TagId {oldTagId} at {DateTime.UtcNow})";

                await container.CreateItemAsync(record, new PartitionKey(newTagId));
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
            string tagId = tag.TagId.Replace(" ", "+");
            tag.TagId = tagId;

            var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.TagId = @tagId"
            ).WithParameter("@tagId", tagId);

            var iterator = container.GetItemQueryIterator<Tag>(queryDefinition);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var item in response)
                {
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

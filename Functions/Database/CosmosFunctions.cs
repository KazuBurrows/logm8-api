using LogMate.Domain.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class CosmosFunctions
    {
        private static readonly string EndpointUri = Environment.GetEnvironmentVariable("CosmosDb_Endpoint");
        private static readonly string PrimaryKey = Environment.GetEnvironmentVariable("CosmosDb_PrimaryKey");
        private static readonly string DatabaseId = "LogmateCosmosDB";
        private static CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

        public static async Task<Record> InsertRecord(Record record)
        {
            string ContainerId = "records";

            try
            {
                var _container = cosmosClient.GetContainer(DatabaseId, ContainerId);
                ItemResponse<Record> response = await _container.CreateItemAsync(
                    record,
                    new PartitionKey(record.TagId)
                );
                return response.Resource;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;
            }
        }

        public static async Task<List<Record>> GetRecordsByTagId(string tagId, ILogger? logger = null)
        {
            string ContainerId = "records";
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.TagId = @TagId ORDER BY c.ServicedDate DESC"
            ).WithParameter("@TagId", tagId);

            var resultSetIterator = container.GetItemQueryIterator<Record>(
                query,
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(tagId) }
            );

            var results = new List<Record>();
            double totalRequestCharge = 0;
            var stopwatch = logger != null ? System.Diagnostics.Stopwatch.StartNew() : null;

            while (resultSetIterator.HasMoreResults)
            {
                var response = await resultSetIterator.ReadNextAsync();
                totalRequestCharge += response.RequestCharge;
                results.AddRange(response);

                if (logger != null && response.Diagnostics.GetClientElapsedTime() > TimeSpan.FromSeconds(1))
                {
                    logger.LogWarning(
                        "Slow Cosmos page in GetRecordsByTagId for {TagId}: {Elapsed}ms, {Diagnostics}",
                        tagId,
                        response.Diagnostics.GetClientElapsedTime().TotalMilliseconds,
                        response.Diagnostics.ToString()
                    );
                }
            }

            if (logger != null)
            {
                stopwatch!.Stop();
                logger.LogInformation(
                    "GetRecordsByTagId for {TagId}: {Count} records, {Elapsed}ms, {RU} RU",
                    tagId,
                    results.Count,
                    stopwatch.ElapsedMilliseconds,
                    totalRequestCharge
                );
            }

            return results;
        }

        public static async Task<Record?> GetRecordById(string id)
        {
            try
            {
                string ContainerId = "records";
                var container = cosmosClient.GetContainer(DatabaseId, ContainerId);
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id").WithParameter(
                    "@id",
                    id
                );

                using FeedIterator<Record> iterator = container.GetItemQueryIterator<Record>(query);

                if (iterator.HasMoreResults)
                {
                    FeedResponse<Record> response = await iterator.ReadNextAsync();
                    return response.Resource.FirstOrDefault();
                }

                return null;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public static async Task<bool> InsertTag(string hash_id)
        {
            Tag data = new Tag
            {
                id = Guid.NewGuid().ToString(),
                TagId = hash_id,
                IsConfigured = false,
            };

            string ContainerId = "tags";
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            try
            {
                var response = await container.CreateItemAsync(data, new PartitionKey(data.TagId));
                return true;
            }
            catch (CosmosException)
            {
                return false;
            }
        }

        public static async Task<bool> UpdateTag(Tag tag)
        {
            string ContainerId = "tags";
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

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

                        await container.ReplaceItemAsync(
                            item,
                            item.id,
                            new PartitionKey(item.TagId)
                        );

                        return true;
                    }
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) { }
            catch (CosmosException) { }
            catch (Exception) { }

            return false;
        }

        public static async Task<Record> PatchRecord(string id, Record updates)
        {
            string containerName = "records";
            var container = cosmosClient.GetContainer(DatabaseId, containerName);

            var operations = new List<PatchOperation>();

            if (updates.ServicedDate != null)
                operations.Add(PatchOperation.Set("/ServicedDate", updates.ServicedDate));

            if (updates.MechanicName != null)
                operations.Add(PatchOperation.Set("/MechanicName", updates.MechanicName));

            if (updates.Odometer != null)
                operations.Add(PatchOperation.Set("/Odometer", updates.Odometer));

            if (updates.ServiceCategory != null)
                operations.Add(PatchOperation.Set("/ServiceCategory", updates.ServiceCategory));

            if (updates.ServiceType != null)
                operations.Add(PatchOperation.Set("/ServiceType", updates.ServiceType));

            if (updates.ServiceOption != null)
                operations.Add(PatchOperation.Set("/ServiceOption", updates.ServiceOption));

            if (updates.Comment != null)
                operations.Add(PatchOperation.Set("/Comment", updates.Comment));

            if (updates.FileUrls != null)
                operations.Add(PatchOperation.Set("/FileUrls", updates.FileUrls));

            var response = await container.PatchItemAsync<Record>(
                id,
                new PartitionKey(updates.TagId),
                operations
            );

            return response.Resource;
        }

        public static async Task<int> ReplaceAllRecordsForTagAsync(
            string oldTagId,
            string newTagId,
            ILogger logger
        )
        {
            var ContainerId = "records";
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.TagId = @oldTagId"
            ).WithParameter("@oldTagId", oldTagId);

            using var iterator = container.GetItemQueryIterator<Record>(
                query,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(oldTagId),
                }
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

            logger.LogInformation(
                "Migrated {Count} records from TagId {Old} → {New}",
                migratedCount,
                oldTagId,
                newTagId
            );

            return migratedCount;
        }

        public static async Task<int> GetNextTagId()
        {
            string ContainerId = "tag_counter";
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            var queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c");

            var queryIterator = container.GetItemQueryIterator<dynamic>(queryDefinition);

            if (queryIterator.HasMoreResults)
            {
                foreach (var item in await queryIterator.ReadNextAsync())
                {
                    return item.next_id;
                }
            }

            return -1;
        }

        public static async Task<bool> UpdateNextTagId()
        {
            string ContainerId = "tag_counter";
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            var queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c");

            var queryIterator = container.GetItemQueryIterator<dynamic>(queryDefinition);

            if (queryIterator.HasMoreResults)
            {
                foreach (var item in await queryIterator.ReadNextAsync())
                {
                    string id = item.id;
                    string partitionKeyValue = item.p_key;
                    item.next_id = item.next_id + 1;

                    await container.ReplaceItemAsync(item, id, new PartitionKey(partitionKeyValue));

                    return true;
                }
            }

            return false;
        }

        public static async Task<int> IsTagConfigured(string id)
        {
            string containerId = "tags";
            var container = cosmosClient.GetContainer(DatabaseId, containerId);

            var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.TagId = @tagId"
            ).WithParameter("@tagId", id);

            var queryIterator = container.GetItemQueryIterator<Tag>(queryDefinition);

            while (queryIterator.HasMoreResults)
            {
                foreach (var item in await queryIterator.ReadNextAsync())
                {
                    return item.IsConfigured ? 1 : 0;
                }
            }

            return -1;
        }

        public static async Task<Tag> GetTagInfo(string tagId, ILogger? logger = null)
        {
            Tag defaultTag = new Tag
            {
                Make = "",
                Model = "",
                Year = 0,
                Vehicle = "",
                Style = "",
                Engine = 0,
                Fuel = [],
                Transmission = "",
                Color = "",
                VinNumber = "",
                LicencePlate = "",
            };

            string ContainerId = "tags";
            var container = cosmosClient.GetContainer(DatabaseId, ContainerId);

            try
            {
                var query = new QueryDefinition(
                    "SELECT TOP 1 * FROM c WHERE c.TagId = @TagId"
                ).WithParameter("@TagId", tagId);

                using var resultSetIterator = container.GetItemQueryIterator<Tag>(
                    query,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(tagId),
                    }
                );

                if (resultSetIterator.HasMoreResults)
                {
                    var response = await resultSetIterator.ReadNextAsync();

                    if (logger != null)
                    {
                        var elapsed = response.Diagnostics.GetClientElapsedTime();
                        logger.LogInformation(
                            "GetTagInfo for {TagId}: {Elapsed}ms, {RU} RU",
                            tagId,
                            elapsed.TotalMilliseconds,
                            response.RequestCharge
                        );

                        if (elapsed > TimeSpan.FromSeconds(1))
                        {
                            logger.LogWarning(
                                "Slow Cosmos call in GetTagInfo for {TagId}: {Diagnostics}",
                                tagId,
                                response.Diagnostics.ToString()
                            );
                        }
                    }

                    var tag = response.FirstOrDefault();

                    if (tag != null)
                    {
                        return tag;
                    }
                }

                return defaultTag;
            }
            catch (CosmosException)
            {
                return defaultTag;
            }
        }
    }
}

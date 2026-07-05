using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function
{
    public partial class LogMateFunction
    {
        public class ServiceOptionResult
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public List<ServiceOptionResult> Children { get; set; } = new List<ServiceOptionResult>();
            public List<string> ServiceTypes { get; set; } = new List<string>();
        }

        public class FlatServiceOption
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public int? ParentId { get; set; }
            public List<string> ServiceTypes { get; set; } = new List<string>();
        }

        public class AddServiceTypeRequest
        {
            public string OptionId { get; set; }
            public string ServiceTypeId { get; set; }
        }

        public class AddParentOptionRequest
        {
            public int OptionId { get; set; }
            public int ParentId { get; set; }
        }

        public class CreateServiceOptionRequest
        {
            public string Name { get; set; }
            public string? Description { get; set; }
            public int CategoryId { get; set; }
        }

        private static readonly SemaphoreSlim _downloadLock = new SemaphoreSlim(1, 1);
        private const string BlobContainerName = "assets";
        private const string BlobFileName = "ServiceOptions.db";
        private static string? _dbPath;
        private static bool _dbReady = false;

        [Function("GetMotorbikeOptions")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext context
        )
        {
            var logger = context.GetLogger("GetMotorbikeOptions");

            const string blobUrl = "https://bloblogmate.blob.core.windows.net/assets/ServiceOptions.db";
            var localDbPath = Path.Combine(Path.GetTempPath(), "ServiceOptions.db");

            try
            {
                await _downloadLock.WaitAsync();
                try
                {
                    if (!File.Exists(localDbPath))
                    {
                        logger.LogInformation("Downloading ServiceOptions.db from blob...");
                        using var http = new HttpClient();
                        using var stream = await http.GetStreamAsync(blobUrl);
                        using var fs = File.Create(localDbPath);
                        await stream.CopyToAsync(fs);
                        logger.LogInformation("Database downloaded successfully.");
                    }
                }
                finally
                {
                    _downloadLock.Release();
                }

                using var source = new SqliteConnection($"Data Source={localDbPath};Mode=ReadOnly;");
                await source.OpenAsync();

                using var memory = new SqliteConnection("Data Source=:memory:;Mode=ReadWrite;");
                await memory.OpenAsync();
                source.BackupDatabase(memory);

                logger.LogInformation("Database loaded into memory.");

                var checkCmd = memory.CreateCommand();
                checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='ServiceOption';";
                var tableName = await checkCmd.ExecuteScalarAsync();
                if (tableName == null)
                    throw new Exception("Table 'ServiceOption' not found in SQLite database.");

                var motorbikeSql =
                    @"
                    WITH RECURSIVE MotorbikeOptions(Id, Name, Description, ParentId) AS (
                        SELECT so.Id, so.Name, so.Description, NULL AS ParentId
                        FROM ServiceOption so
                        JOIN VehicleTypeServiceOption vtso ON vtso.ServiceOptionId = so.Id
                        JOIN VehicleType vt ON vt.Id = vtso.VehicleTypeId
                        WHERE vt.Name = 'Motorbike'
                        UNION ALL
                        SELECT child.Id, child.Name, child.Description, rel.ParentId
                        FROM ServiceOptionRelation rel
                        JOIN ServiceOption child ON child.Id = rel.ChildId
                        JOIN MotorbikeOptions m ON m.Id = rel.ParentId
                    )
                    SELECT mo.Id, mo.Name, mo.Description, mo.ParentId,
                        GROUP_CONCAT(st.Name) AS ServiceTypes
                    FROM MotorbikeOptions mo
                    LEFT JOIN ServiceOptionServiceType sst ON sst.ServiceOptionId = mo.Id
                    LEFT JOIN ServiceType st ON st.Id = sst.ServiceTypeId
                    GROUP BY mo.Id, mo.Name, mo.Description, mo.ParentId;
                    ";

                var flatResults = new List<FlatServiceOption>();
                using (var cmd = memory.CreateCommand())
                {
                    cmd.CommandText = motorbikeSql;
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        flatResults.Add(new FlatServiceOption
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ParentId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            ServiceTypes = reader.IsDBNull(4)
                                ? new List<string>()
                                : reader.GetString(4).Split(',').ToList(),
                        });
                    }
                }

                var ownershipSql =
                    @"
                    SELECT so.Id, so.Name, so.Description, NULL AS ParentId,
                        GROUP_CONCAT(st.Name) AS ServiceTypes
                    FROM ServiceOption so
                    LEFT JOIN ServiceOptionServiceType sst ON sst.ServiceOptionId = so.Id
                    LEFT JOIN ServiceType st ON st.Id = sst.ServiceTypeId
                    WHERE so.Name = 'Ownership'
                    GROUP BY so.Id, so.Name, so.Description;
                    ";

                var ownershipResults = new List<FlatServiceOption>();
                using (var cmd = memory.CreateCommand())
                {
                    cmd.CommandText = ownershipSql;
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        ownershipResults.Add(new FlatServiceOption
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ParentId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            ServiceTypes = reader.IsDBNull(4)
                                ? new List<string>()
                                : reader.GetString(4).Split(',').ToList(),
                        });
                    }
                }

                List<ServiceOptionResult> BuildHierarchy(List<FlatServiceOption> flatList)
                {
                    var dict = flatList
                        .GroupBy(x => x.Id)
                        .Select(g => new ServiceOptionResult
                        {
                            Id = g.Key,
                            Name = g.First().Name,
                            Description = g.First().Description,
                            ServiceTypes = g.SelectMany(f => f.ServiceTypes).Distinct().ToList(),
                        })
                        .ToDictionary(x => x.Id);

                    foreach (var node in flatList)
                    {
                        if (node.ParentId.HasValue && dict.ContainsKey(node.ParentId.Value))
                            dict[node.ParentId.Value].Children.Add(dict[node.Id]);
                    }

                    return dict
                        .Values.Where(x => !flatList.Any(r => r.Id == x.Id && r.ParentId.HasValue))
                        .ToList();
                }

                var combined = new
                {
                    MotorbikeOptions = BuildHierarchy(flatResults),
                    OwnershipOptions = BuildHierarchy(ownershipResults),
                };

                var json = JsonConvert.SerializeObject(combined, Formatting.Indented);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(json);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync($"Server error: {ex.Message}");
                return response;
            }
        }

        [Function("UpdateServiceOptionsDb")]
        public async Task<HttpResponseData> UpdateServiceOptionsDb(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext context
        )
        {
            var logger = context.GetLogger("UpdateServiceOptionsDb");

            const string blobUrl = "https://bloblogmate.blob.core.windows.net/assets/ServiceOptions.db";
            var localDbPath = Path.Combine(Path.GetTempPath(), "ServiceOptions.db");

            try
            {
                await _downloadLock.WaitAsync();
                try
                {
                    logger.LogInformation("Forcing update of ServiceOptions.db from blob...");
                    using var http = new HttpClient();
                    using var stream = await http.GetStreamAsync(blobUrl);
                    using var fs = File.Create(localDbPath);
                    await stream.CopyToAsync(fs);
                    logger.LogInformation("Database file updated successfully.");
                }
                finally
                {
                    _downloadLock.Release();
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync("ServiceOptions.db has been updated.");
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error updating DB: {ex}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync($"Failed to update DB: {ex.Message}");
                return response;
            }
        }

        [Function("GetServiceRecordHierarchy")]
        public async Task<HttpResponseData> GetServiceRecordHierarchy(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext context
        )
        {
            var logger = context.GetLogger("GetServiceRecordHierarchy");

            try
            {
                var result = await SqlFunctions.GetHierarchy();

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonConvert.SerializeObject(result, Formatting.Indented));

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get service record hierarchy");

                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Failed to get service record hierarchy");
                return response;
            }
        }

        [Function("AddServiceType")]
        public async Task<HttpResponseData> AddServiceType(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext context
        )
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var payload = JsonConvert.DeserializeObject<AddServiceTypeRequest>(body);

            if (payload == null || string.IsNullOrEmpty(payload.OptionId))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid payload");
                return bad;
            }

            var inserted = await SqlFunctions.AddServiceTypeById(payload.OptionId, payload.ServiceTypeId);

            var response = req.CreateResponse(inserted ? HttpStatusCode.OK : HttpStatusCode.Conflict);
            await response.WriteStringAsync(
                inserted ? "Service type linked successfully" : "Service type already exists"
            );
            return response;
        }

        [Function("AddParentOption")]
        public async Task<HttpResponseData> AddParentOption(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext context
        )
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var payload = JsonConvert.DeserializeObject<AddParentOptionRequest>(body);

            if (payload == null || payload.OptionId <= 0 || payload.ParentId <= 0)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid payload");
                return bad;
            }

            var inserted = await SqlFunctions.AddParentOptionById(payload.OptionId, payload.ParentId);

            var response = req.CreateResponse(inserted ? HttpStatusCode.OK : HttpStatusCode.Conflict);
            await response.WriteStringAsync(
                inserted ? "Parent option linked successfully" : "Parent option already exists"
            );
            return response;
        }

        [Function("CreateServiceOption")]
        public async Task<HttpResponseData> CreateServiceOption(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext context
        )
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var payload = JsonConvert.DeserializeObject<CreateServiceOptionRequest>(body);

            if (payload == null || string.IsNullOrWhiteSpace(payload.Name))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid payload");
                return bad;
            }

            var inserted = await SqlFunctions.CreateNewServiceOption(
                payload.Name.Trim(),
                payload.Description,
                payload.CategoryId
            );

            var response = req.CreateResponse(inserted ? HttpStatusCode.OK : HttpStatusCode.Conflict);
            await response.WriteStringAsync(
                inserted ? "Service option created successfully" : "Service option name already exists"
            );
            return response;
        }
    }
}

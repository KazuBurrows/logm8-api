using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Company.Function;

public class MigrateTagRecords
{
    private readonly ILogger<MigrateTagRecords> _logger;

    private const string OldTagId = "VDAN+48mLLh8f52YLvy8sq11I0OcC1cLa1g68Kp94wU=";
    private const string NewTagId = "bkABhxwM8nx2NO8dxHhAj2QkEP06RE4qiOMB9cSjWk0=";

    public MigrateTagRecords(ILogger<MigrateTagRecords> logger)
    {
        _logger = logger;
    }

    [Function("MigrateTagRecords")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req
    )
    {
        _logger.LogInformation("MigrateTagRecords triggered. Migrating {Old} → {New}", OldTagId, NewTagId);

        var count = await CosmosFunctions.ReplaceAllRecordsForTagAsync(OldTagId, NewTagId, _logger);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"Successfully migrated {count} record(s).");
        return response;
    }
}

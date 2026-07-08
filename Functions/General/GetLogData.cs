using LogMate.Application.Exceptions;
using LogMate.Domain.Models;
using System.Net;
using System.Text.Json.Nodes;
using Company.Function;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class GetLogData
{
    private readonly ILogger<GetLogData> _logger;

    public GetLogData(ILogger<GetLogData> logger)
    {
        _logger = logger;
    }

    [Function("GetLogData")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("GetLogData function triggered.");

        req.Headers.TryGetValues("Authorization", out var authValues);
        req.Headers.TryGetValues("X-Tag-Id", out var tagIdValues);
        string? authorization = authValues?.FirstOrDefault();
        string? tagIdHeader = tagIdValues?.FirstOrDefault();

        _logger.LogInformation("Authorization: {Authorization}", authorization);
        _logger.LogInformation("X-Tag-Id: {TagIdHeader}", tagIdHeader);

        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        string tokenKey = query["token"];

        if (string.IsNullOrEmpty(tokenKey))
        {
            _logger.LogWarning("Token is missing from request.");
            throw new ApiException(HttpStatusCode.BadRequest, "Missing token.");
        }

        _logger.LogInformation("Processing token: {Token}", tokenKey);

        string str_log = await SqlFunctions.IsOneLifeUrlExpired(tokenKey, _logger);

        if (string.IsNullOrEmpty(str_log))
        {
            _logger.LogWarning("SQL returned null/empty for token: {Token}", tokenKey);
            throw new ApiException(HttpStatusCode.NotFound, "Not found.");
        }

        var log = JsonNode.Parse(str_log)?.AsObject();

        string logId = log?["LogId"]?.GetValue<string>();
        DateTime? ttl = log?["TTL"]?.GetValue<DateTime?>();

        _logger.LogInformation("Parsed logId: {LogId}, TTL: {TTL}", logId, ttl);

        if (logId == "Not Found" || !ttl.HasValue || ttl.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired token. LogId: {LogId}, TTL: {TTL}", logId, ttl);
            throw new ApiException(HttpStatusCode.NotFound, "Not found.");
        }

        _logger.LogInformation("Fetching Cosmos data for LogId: {LogId}", logId);

        Task<Tag> tagTask = CosmosFunctions.GetTagInfo(logId, _logger);
        Task<List<Record>> recordsTask = CosmosFunctions.GetRecordsByTagId(logId, _logger);
        await Task.WhenAll(tagTask, recordsTask);

        Tag local_tag = tagTask.Result;
        List<Record> list_records = recordsTask.Result;
        int? view_mode = log?["Mode"]?.GetValue<int>();

        _logger.LogInformation("Fetched {RecordCount} records for LogId: {LogId}", list_records?.Count ?? 0, logId);

        var response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(new
        {
            tag = local_tag,
            records = list_records,
            mode = view_mode
        });

        _logger.LogInformation("Response successfully returned for LogId: {LogId}", logId);

        return response;
    }
}
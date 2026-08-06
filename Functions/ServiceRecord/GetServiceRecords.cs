using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using System.Net;
using System.Text.Json;
using Company.Function;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

public class GetServiceRecords
{
    private readonly IServiceRecordService _service;

    public GetServiceRecords(IServiceRecordService service)
    {
        _service = service;
    }

    [Function("GetServiceRecords")]         // NOT GETTING USED
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req
    )
    {
        // Get query param
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        string token = query["token"];

        if (string.IsNullOrEmpty(token))
            throw new ApiException(HttpStatusCode.BadRequest, "Missing token parameter");

        var (logId, _) = await SqlFunctions.EnsureOneLifeTokenNotExpiredAsync(token);

        var records = await CosmosFunctions.GetRecordsByTagId(logId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        await response.WriteStringAsync(JsonSerializer.Serialize(records));

        return response;
    }
}

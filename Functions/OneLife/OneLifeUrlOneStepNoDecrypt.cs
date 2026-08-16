using Company.Function;
using LogMate.Application.Exceptions;
using LogMate.Common;
using LogMate.Common.Http;
using LogMate.Domain.Models;
using System.Text.Json.Nodes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class OneLifeUrlOneStepNoDecrypt
{
    private readonly ILogger<OneLifeUrlOneStepNoDecrypt> _logger;

    public OneLifeUrlOneStepNoDecrypt(ILogger<OneLifeUrlOneStepNoDecrypt> logger)
    {
        _logger = logger;
    }

    [Function("OneLifeUrlOneStepNoDecrypt")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        string? reqString = query["reqString"];
        JsonNode reversedJson = JsonNode.Parse(reqString!)!;

        string? eId = reversedJson["eId"]?.ToString();

        if (string.IsNullOrEmpty(eId))
            throw new BadRequestException("Missing eId.");

        string id = eId.Replace(" ", "+");

        int isConfigured = await CosmosFunctions.IsTagConfigured(id);

        if (isConfigured == -1)
            throw new NotFoundException("Tag not found.");

        string url = isConfigured == 1
            ? $"{AppConstants.BaseLogm8Url}/log?token={await SqlFunctions.GenerateOneLifeUrlAsync(id, (int)UserMode.Service, null)}"
            : $"{AppConstants.BaseLogm8Url}/tag/create?token={id}";

        return await ApiResponseFactory.Ok(
            req,
            "OneLife URL resolved successfully.",
            new Dictionary<string, object> { { "success", isConfigured == 1 }, { "url", url } }
        );
    }
}

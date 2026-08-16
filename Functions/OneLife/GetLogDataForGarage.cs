using Company.Function;
using LogMate.Application.Exceptions;
using LogMate.Common.Crypto;
using LogMate.Domain.Models;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class GetLogDataForGarage
{
    private readonly ILogger<GetLogDataForGarage> _logger;

    public GetLogDataForGarage(ILogger<GetLogDataForGarage> logger)
    {
        _logger = logger;
    }

    [Function("GetLogDataForGarage")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        string reqString = req.Query["reqString"];
        JsonNode reversedJson = JsonNode.Parse(reqString)!;

        string? eId = reversedJson["eId"]?.ToString();

        if (string.IsNullOrEmpty(eId))
            throw new BadRequestException("Missing eId.");

        var result = AesTokenCipher.Decrypt(eId);
        string id = result.DecryptedText.Replace(" ", "+");

        int isConfigured = await CosmosFunctions.IsTagConfigured(id);
        if (isConfigured == 0)
        {
            return new OkObjectResult(new { tag = "Not Found" });
        }

        Tag localTag = await CosmosFunctions.GetTagInfo(id);
        localTag.TagId = "";
        return new OkObjectResult(new { tag = localTag });
    }
}

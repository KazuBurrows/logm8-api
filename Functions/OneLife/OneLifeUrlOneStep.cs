using Company.Function;
using LogMate.Application.Exceptions;
using LogMate.Common;
using LogMate.Common.Crypto;
using LogMate.Domain.Models;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class OneLifeUrlOneStep
{
    private readonly ILogger<OneLifeUrlOneStep> _logger;

    public OneLifeUrlOneStep(ILogger<OneLifeUrlOneStep> logger)
    {
        _logger = logger;
    }

    [Function("OneLifeUrlOneStep")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        string reqString = req.Query["reqString"];
        JsonNode reversedJson = JsonNode.Parse(reqString)!;

        string? eId = reversedJson["eId"]?.ToString();
        string? userId = reversedJson["userId"]?.ToString();

        if (string.IsNullOrEmpty(eId))
            throw new BadRequestException("Missing eId.");

        var result = AesTokenCipher.Decrypt(eId);
        string id = result.DecryptedText.Replace(" ", "+");

        int isConfigured = await CosmosFunctions.IsTagConfigured(id);

        if (isConfigured == 1)
        {
            string tokenUrl = await SqlFunctions.GenerateOneLifeUrlAsync(id, (int)UserMode.Service, userId);
            return new OkObjectResult($"{AppConstants.BaseLogm8Url}/log?token=" + tokenUrl);
        }
        else if (isConfigured == -1)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult($"{AppConstants.BaseLogm8Url}/tag/create?token=" + id);
    }
}

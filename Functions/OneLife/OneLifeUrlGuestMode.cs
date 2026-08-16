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

public class OneLifeUrlGuestMode
{
    private readonly ILogger<OneLifeUrlGuestMode> _logger;

    public OneLifeUrlGuestMode(ILogger<OneLifeUrlGuestMode> logger)
    {
        _logger = logger;
    }

    [Function("OneLifeUrlGuestMode")]
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

        string tokenUrl = await SqlFunctions.GenerateOneLifeUrlAsync(id, (int)UserMode.Guest, null);
        return new OkObjectResult($"{AppConstants.BaseLogm8Url}/log?token=" + tokenUrl);
    }
}

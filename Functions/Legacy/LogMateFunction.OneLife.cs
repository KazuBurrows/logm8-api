using LogMate.Application.Exceptions;
using LogMate.Common.Http;
using LogMate.Domain.Models;
using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public partial class LogMateFunction
    {
        [Function("OneLifeUrl")]
        public async Task<IActionResult> OneLifeUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            string accessToken = reversedJson["accessToken"]?.ToString();
            string eId = reversedJson["eId"]?.ToString();

            string privateKey = await SqlFunctions.GetPrivateKey(accessToken);
            var rsa = System.Security.Cryptography.RSA.Create();
            string id = DecryptText(rsa, eId, privateKey);

            int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            if (isConfigured == 1)
            {
                string tokenUrl = await SqlFunctions.GenerateOneLifeUrlAsync(id, (int)UserMode.Service, null);
                return new OkObjectResult($"{BaseLogm8Url}/log?token={tokenUrl}");
            }
            else if (isConfigured == -1)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult($"{BaseLogm8Url}/tag/create?token={id}");
        }

        [Function("OneLifeUrlNoEncrypt")]
        public async Task<IActionResult> OneLifeUrlNoEncrypt(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            string accessToken = reversedJson["accessToken"]?.ToString();
            string eId = reversedJson["eId"]?.ToString();

            _logger.LogInformation("eId: " + eId);
            string id = eId.Replace(" ", "+");

            int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            if (isConfigured == 1)
            {
                string tokenUrl = await SqlFunctions.GenerateOneLifeUrlAsync(id, (int)UserMode.Service, null);
                return new OkObjectResult($"{BaseLogm8Url}/log?token=" + tokenUrl);
            }
            else if (isConfigured == -1)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult($"{BaseLogm8Url}/tag/create?token=" + id);
        }

        [Function("OneLifeUrlOneStep")]
        public async Task<IActionResult> OneLifeUrlOneStep(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            string eId = reversedJson["eId"]?.ToString();
            string userId = reversedJson["userId"]?.ToString();

            _logger.LogInformation("userId: " + userId);

            var result = DecryptString(eId);

            _logger.LogInformation("eId: " + result.decryptedText);
            string id = result.decryptedText.Replace(" ", "+");
            _logger.LogInformation("id: " + id);

            int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            _logger.LogInformation("isConfigured: " + isConfigured);

            if (isConfigured == 1)
            {
                string tokenUrl = await SqlFunctions.GenerateOneLifeUrlAsync(id, (int)UserMode.Service, userId);
                return new OkObjectResult($"{BaseLogm8Url}/log?token=" + tokenUrl);
            }
            else if (isConfigured == -1)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult($"{BaseLogm8Url}/tag/create?token=" + id);
        }

        [Function("OneLifeUrlGuestMode")]
        public async Task<IActionResult> OneLifeUrlGuestMode(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            string eId = reversedJson["eId"]?.ToString();

            var result = DecryptString(eId);

            _logger.LogInformation("eId: " + result.decryptedText);
            string id = result.decryptedText.Replace(" ", "+");
            _logger.LogInformation("id: " + id);

            string tokenUrl = await SqlFunctions.GenerateOneLifeUrlAsync(id, (int)UserMode.Guest, null);
            _logger.LogInformation("tokenUrl: " + tokenUrl);
            return new OkObjectResult($"{BaseLogm8Url}/log?token=" + tokenUrl);
        }

        [Function("OneLifeUrlOneStepNoDecrypt")]
        public async Task<IActionResult> OneLifeUrlOneStepNoDecrypt(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            string result = reversedJson["eId"]?.ToString();

            _logger.LogInformation("eId: " + result);
            string id = result.Replace(" ", "+");
            _logger.LogInformation("id: " + id);

            int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            _logger.LogInformation("isConfigured: " + isConfigured);

            if (isConfigured == -1)
                throw new NotFoundException("Tag not found.");

            string url = isConfigured == 1
                ? $"{BaseLogm8Url}/log?token={await SqlFunctions.GenerateOneLifeUrlAsync(id, (int)UserMode.Service, null)}"
                : $"{BaseLogm8Url}/tag/create?token={id}";

            var body = ApiResponseFactory.Build(
                HttpStatusCode.OK,
                "Success",
                "OneLife URL resolved successfully.",
                new Dictionary<string, object> { { "success", isConfigured == 1 }, { "url", url } }
            );

            return new ObjectResult(body) { StatusCode = (int)HttpStatusCode.OK };
        }

        [Function("ConsumeOneLifeUrl")]
        public async Task<IActionResult> ConsumeOneLifeUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string tokenKey = req.Query["token"];
            await SqlFunctions.CommitOneLifeUrlComsumed(tokenKey);
            return new OkObjectResult("");
        }

        [Function("GetLogDataForGarage")]
        public async Task<IActionResult> GetLogDataForGarage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            string reqString = req.Query["reqString"];
            JsonNode reversedJson = JsonNode.Parse(reqString)!;

            string eId = reversedJson["eId"]?.ToString();

            var result = DecryptString(eId);

            _logger.LogInformation("eId: " + result.decryptedText);
            string id = result.decryptedText.Replace(" ", "+");

            int isConfigured = await CosmosFunctions.IsTagConfigured(id);
            if (isConfigured == 0)
            {
                return new OkObjectResult(new { tag = "Not Found" });
            }

            Tag local_tag = await CosmosFunctions.GetTagInfo(id);
            local_tag.TagId = "";
            return new OkObjectResult(new { tag = local_tag });
        }
    }
}

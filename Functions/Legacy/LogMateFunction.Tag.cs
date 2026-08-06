using LogMate.Application.Exceptions;
using LogMate.Common.Http;
using LogMate.Domain.Models;
using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function
{
    public partial class LogMateFunction
    {
        [Function("AcquireNewTag")]
        public async Task<IActionResult> AcquireNewTag(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req
        )
        {
            int id = await CosmosFunctions.GetNextTagId();
            if (id == -1)
                return new NotFoundResult();

            string str_id = id.ToString();
            _logger.LogInformation("str_id: " + str_id);

            string hash_id = HashWithPepper(str_id, pepper);

            bool isInserted = await CosmosFunctions.InsertTag(hash_id);
            bool isUpdated = await CosmosFunctions.UpdateNextTagId();
            if (isUpdated == false)
                return new NotFoundResult();
            if (isInserted)
                return new OkObjectResult($"{hash_id}");

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        [Function("SubmitTag")]
        public async Task<IActionResult> SubmitTag(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req
        )
        {
            string? authorization = req.Headers["Authorization"].FirstOrDefault();
            string? tagIdHeader = req.Headers["X-Tag-Id"].FirstOrDefault();

            _logger.LogInformation("Authorization: {Authorization}", authorization);
            _logger.LogInformation("X-Tag-Id: {TagIdHeader}", tagIdHeader);

            string str_tag = req.Query["tag"];
            _logger.LogInformation("str_tag: " + str_tag);

            Tag tag = JsonConvert.DeserializeObject<Tag>(str_tag);
            _logger.LogInformation("tag: " + tag.TagId);

            bool status = await CosmosFunctions.UpdateTag(tag);
            _logger.LogInformation("status: " + status);

            if (!status)
                throw new NotFoundException("Tag not found or could not be updated.");

            var body = ApiResponseFactory.Build(
                HttpStatusCode.OK,
                "Successfully Updated",
                "Tag inserted successfully."
            );

            return new ObjectResult(body) { StatusCode = (int)HttpStatusCode.OK };
        }

        [Function("UpdateTag")]
        public async Task<IActionResult> UpdateTag(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req
        )
        {
            string str_tag = req.Query["tag"];
            _logger.LogInformation("str_tag: " + str_tag);

            Tag tag = JsonConvert.DeserializeObject<Tag>(str_tag);
            _logger.LogInformation("tag: " + tag.TagId);

            string token = tag.TagId;
            string str_log = await SqlFunctions.IsOneLifeUrlConsumed(token);
            var log = JsonNode.Parse(str_log).AsObject();
            tag.TagId = (string)log["LogId"];

            _logger.LogInformation("tag: " + tag.TagId);

            bool status = await CosmosFunctions.UpdateTag(tag);
            _logger.LogInformation("status: " + status);

            return new OkObjectResult(new
            {
                success = status,
                message = status ? "Successful Tag Insert" : "Failed to insert tag",
            });
        }
    }
}

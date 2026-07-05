using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function
{
    public partial class LogMateFunction
    {
        public class ReplaceTagRequest
        {
            public string OldTagId { get; set; } = default!;
            public string NewTagId { get; set; } = default!;
        }

        [Function("ReplaceNfcTag")]
        public async Task<HttpResponseData> ReplaceNfcTag(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext context
        )
        {
            var logger = context.GetLogger("ReplaceNfcTag");

            req.Headers.TryGetValues("Authorization", out var authValues);
            req.Headers.TryGetValues("X-Tag-Id", out var tagIdValues);
            string? authorization = authValues?.FirstOrDefault();
            string? tagIdHeader = tagIdValues?.FirstOrDefault();

            logger.LogInformation("Authorization: {Authorization}", authorization);
            logger.LogInformation("X-Tag-Id: {TagIdHeader}", tagIdHeader);

            ReplaceTagRequest? request;

            try
            {
                using var reader = new StreamReader(req.Body);
                var body = await reader.ReadToEndAsync();
                request = JsonConvert.DeserializeObject<ReplaceTagRequest>(body);
            }
            catch
            {
                return await CreateResponseAsync(req, HttpStatusCode.BadRequest, "Invalid JSON payload");
            }

            if (
                request == null
                || string.IsNullOrWhiteSpace(request.OldTagId)
                || string.IsNullOrWhiteSpace(request.NewTagId)
            )
            {
                return await CreateResponseAsync(req, HttpStatusCode.BadRequest, "OldTagId and NewTagId are required");
            }

            try
            {
                var count = await CosmosFunctions.ReplaceAllRecordsForTagAsync(
                    request.OldTagId,
                    request.NewTagId,
                    logger
                );

                return await CreateResponseAsync(req, HttpStatusCode.OK, $"Successfully migrated {count} records.");
            }
            catch (CosmosException ex)
            {
                logger.LogError(ex, "Cosmos DB failure");
                return await CreateResponseAsync(req, HttpStatusCode.InternalServerError, "Database operation failed");
            }
        }
    }
}

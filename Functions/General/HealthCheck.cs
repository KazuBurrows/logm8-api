using LogMate.Common.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace LogMate.Functions.General;

public class HealthCheck
{
    [Function("HealthCheck")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        return await ApiResponseFactory.Ok(req, "LogMate API is healthy.");
    }
}

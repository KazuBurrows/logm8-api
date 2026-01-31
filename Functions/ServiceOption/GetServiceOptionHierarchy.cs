using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class GetServiceOptionHierarchy
{
    private readonly IServiceOptionService _service;

    public GetServiceOptionHierarchy(IServiceOptionService service)
    {
        _service = service;
    }

    [Function("GetServiceOptionHierarchy")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
        FunctionContext context
    )
    {
        var logger = context.GetLogger("GetServiceOptionHierarchy");

        try
        {
            var result = await _service.GetServiceOptionHierarchyAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(
                JsonConvert.SerializeObject(result, Formatting.Indented)
            );

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get service record hierarchy");

            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Failed to get service record hierarchy");
            return response;
        }
    }
}

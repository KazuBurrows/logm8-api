using LogMate.Application.Interfaces;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req
    )
    {
        var result = await _service.GetServiceOptionHierarchyAsync();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(
            JsonConvert.SerializeObject(result, Formatting.Indented)
        );

        return response;
    }
}

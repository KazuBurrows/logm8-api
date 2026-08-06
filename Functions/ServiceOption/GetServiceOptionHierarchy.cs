using LogMate.Application.Interfaces;
using LogMate.Common.Http;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

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

        return await ApiResponseFactory.Ok(
            req,
            "Service option hierarchy retrieved successfully.",
            new Dictionary<string, object>
            {
                { "MotorbikeOptions", result.MotorbikeOptions },
                { "OwnershipOptions", result.OwnershipOptions },
            }
        );
    }
}

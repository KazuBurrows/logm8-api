using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Domain.Models;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

public class AddServiceOptionServiceType
{
    private readonly IServiceOptionService _service;

    public AddServiceOptionServiceType(IServiceOptionService service)
    {
        _service = service;
    }

    [Function("AddServiceOptionServiceType")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        var payload = JsonConvert.DeserializeObject<AddServiceTypeRequest>(body);

        if (payload == null || string.IsNullOrEmpty(payload.OptionId))
            throw new ApiException(HttpStatusCode.BadRequest, "Invalid payload");

        var status = await _service.AddServiceOptionServiceTypeAsync(payload);

        if (status == HttpStatusCode.Conflict)
            throw new ApiException(HttpStatusCode.Conflict, "Service type already exists");

        if (status != HttpStatusCode.OK)
            throw new ApiException(HttpStatusCode.InternalServerError, "Failed to link service type");

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Service type linked successfully");

        return response;
    }
}

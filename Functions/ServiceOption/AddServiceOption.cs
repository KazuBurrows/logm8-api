using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Domain.Models;
using LogMate.Infrastructure.Extensions;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

public class AddServiceOption
{
    private readonly IServiceOptionService _service;

    public AddServiceOption(IServiceOptionService service)
    {
        _service = service;
    }

    [Function("AddServiceOption")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        var payload = JsonConvert.DeserializeObject<AddServiceOptionRequest>(body);

        if (payload.Name.IsNullOrEmpty())
            throw new ApiException(HttpStatusCode.BadRequest, "Invalid payload");

        var status = await _service.AddServiceOptionAsync(payload);

        if (status == HttpStatusCode.Conflict)
            throw new ApiException(HttpStatusCode.Conflict, "Service option name already exists");

        if (status != HttpStatusCode.OK)
            throw new ApiException(HttpStatusCode.InternalServerError, "Failed to create service option");

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Service option created successfully");

        return response;
    }
}

using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Common.Http;
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
            throw new BadRequestException("Invalid payload");

        var status = await _service.AddServiceOptionServiceTypeAsync(payload);

        if (status == HttpStatusCode.Conflict)
            throw new ConflictException(
                "Service type already exists",
                new List<int> { int.Parse(payload.OptionId), int.Parse(payload.ServiceTypeId) }
            );

        return await ApiResponseFactory.Ok(req, "Service type linked successfully");
    }
}

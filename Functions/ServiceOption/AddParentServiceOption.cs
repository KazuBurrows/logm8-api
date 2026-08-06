using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Common.Http;
using LogMate.Domain.Models;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

public class AddParentServiceOption
{
    private readonly IServiceOptionService _service;

    public AddParentServiceOption(IServiceOptionService service)
    {
        _service = service;
    }

    [Function("AddParentServiceOption")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        var payload = JsonConvert.DeserializeObject<AddParentOptionRequest>(body);

        if (payload == null || payload.OptionId <= 0 || payload.ParentId <= 0)
            throw new BadRequestException("Invalid payload");

        var status = await _service.AddParentServiceOptionAsync(payload);

        if (status == HttpStatusCode.Conflict)
            throw new ConflictException("Parent option already exists", new List<int> { payload.OptionId, payload.ParentId });

        return await ApiResponseFactory.Ok(req, "Parent option linked successfully");
    }
}

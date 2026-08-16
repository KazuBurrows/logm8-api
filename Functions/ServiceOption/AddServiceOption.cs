using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Common.Http;
using LogMate.Common.Validation;
using LogMate.Domain.Models;
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
        var payload = ModelValidator.Validate(JsonConvert.DeserializeObject<AddServiceOptionRequest>(body));

        var (status, id) = await _service.AddServiceOptionAsync(payload);

        if (status == HttpStatusCode.Conflict)
            throw new ConflictException("Service option name already exists", new List<int> { id });

        return await ApiResponseFactory.Success(req, "Service option", id, ActionType.Created);
    }
}

using System.Net;
using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Common.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

public class SubmitRecord
{
    private readonly IServiceRecordService _service;

    public SubmitRecord(IServiceRecordService service)
    {
        _service = service;
    }

    [Function("SubmitRecord")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req
    )
    {
        var formData = await req.ReadFormAsync();
        if (formData == null)
            throw new BadRequestException("Invalid form data");

        var record = await _service.AddServiceRecordAsync(formData);

        var body = ApiResponseFactory.Build(
            HttpStatusCode.Created,
            "Successfully Created",
            "Service record was successfully created.",
            new Dictionary<string, object> { { "item", record! } }
        );

        return new ObjectResult(body) { StatusCode = (int)HttpStatusCode.Created };
    }
}

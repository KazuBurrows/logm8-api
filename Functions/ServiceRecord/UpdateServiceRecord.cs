using System.Net;
using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

public class UpdateServiceRecord
{
    private readonly IServiceRecordService _service;

    public UpdateServiceRecord(IServiceRecordService service)
    {
        _service = service;
    }

    [Function("UpdateServiceRecord")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req
    )
    {
        var formData = await req.ReadFormAsync();
        if (formData == null)
            throw new ApiException(HttpStatusCode.BadRequest, "Invalid form data");

        var record = await _service.UpdateServiceRecordAsync(formData);

        return new OkObjectResult(
            new { success = true, message = "Service record updated successfully", data = record }
        );
    }
}

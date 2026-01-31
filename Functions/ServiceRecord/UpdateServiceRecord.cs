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
        {
            return new BadRequestObjectResult("Invalid form data");
        }

        var result = await _service.UpdateServiceRecordAsync(formData);

        return result switch
        {
            true  => new OkObjectResult("Service record updated successfully"),
            false => new BadRequestObjectResult("Bad request data"),
            null  => new NotFoundObjectResult("Service record not found"),
        };
    }
}

using System.IO;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

public class AddServiceRecord
{
    private readonly IServiceRecordService _service;

    public AddServiceRecord(IServiceRecordService service)
    {
        _service = service;
    }

    [Function("AddServiceRecord")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(
                    new { success = false, message = "Request body is empty" }
                );
                return bad;
            }

            var request = JsonConvert.DeserializeObject<AddServiceRecordRequest>(body);

            if (request == null)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(
                    new { success = false, message = "Invalid JSON payload" }
                );
                return bad;
            }

            var serviceRequest = new ServiceRecordRequest
            {
                Token = request.Token,
                EnteredDate = request.EnteredDate,
                ServicedDate = request.ServicedDate,
                MechanicName = request.MechanicName,
                Odometer = request.Odometer,
                ServiceCategory = request.ServiceCategory,
                ServiceType = request.ServiceType,
                ServiceOption = request.ServiceOption,
                Comment = request.Comment
            };

            var success = await _service.AddServiceRecordAsync(serviceRequest);


            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(
                new
                {
                    success,
                    message = success
                        ? "Service record added successfully"
                        : "Failed to add service record",
                }
            );

            return response;
        }
        catch (Exception ex)
        {
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { success = false, message = ex.Message });
            return response;
        }
    }

}

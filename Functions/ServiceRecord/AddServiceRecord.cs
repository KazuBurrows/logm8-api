using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Domain.Models;
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
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
            throw new ApiException(HttpStatusCode.BadRequest, "Request body is empty");

        var request = JsonConvert.DeserializeObject<AddServiceRecordRequest>(body);

        if (request == null)
            throw new ApiException(HttpStatusCode.BadRequest, "Invalid JSON payload");

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

        var record = await _service.AddServiceRecordAsync(serviceRequest);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(
            new
            {
                success = true,
                message = "Service record added successfully",
                data = record,
            }
        );

        return response;
    }
}

using System.Net;
using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Domain.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace LogMate.Functions.ServiceRecord;

public class ServiceRecordCollection
{
    private readonly IServiceRecordService _service;

    public ServiceRecordCollection(IServiceRecordService serviceRecordService)
    {
        _service = serviceRecordService;
    }

    [Function("ServiceRecordCollection")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get", "post", "options",
            Route = "serviceRecords")] HttpRequestData req,
        FunctionContext context)
    {
        if (req.Method == "GET")
        {
            var serviceRecords = await _service.GetAll();
            var okResponse = req.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteAsJsonAsync(new { success = true, data = serviceRecords });
            return okResponse;
        }

        if (req.Method == "POST")
        {
            var formData = await req.ReadFromJsonAsync<Record>();

            if (formData == null)
                throw new ApiException(HttpStatusCode.BadRequest, "Invalid form data");

            var record = await _service.Create(formData);

            if (record == null)
                throw new ApiException(HttpStatusCode.InternalServerError, "Failed to add service record");

            var createdResponse = req.CreateResponse(HttpStatusCode.Created);
            await createdResponse.WriteAsJsonAsync(new { success = true, message = "Service record added successfully", data = record });
            return createdResponse;
        }

        throw new ApiException(HttpStatusCode.MethodNotAllowed, "Method not allowed");
    }
}

using System.Net;
using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Common.Http;
using LogMate.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace LogMate.Functions.ServiceRecord;

public class ServiceRecordItem(IServiceRecordService serviceRecord, INfcTagService nfcTagService)
{
    private readonly IServiceRecordService _service = serviceRecord;
    private readonly INfcTagService _nfcTagService = nfcTagService;

    [Function("ServiceRecordItem")]
    public async Task<IActionResult> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get", "delete", "put", "options",
            Route = "servicerecord/{id}")] HttpRequest req,
        string id)
    {
        if (req.Method == "GET")
        {
            var serviceRecord = await _service.GetById(id);

            if (serviceRecord == null)
                throw new ApiException(HttpStatusCode.NotFound, "Service record not found");

            // token is optional here: with it, the response includes an accurate canEdit;
            // without it, canEdit always comes back false (no caller identity to compare against).
            string? token = req.Query["token"];
            string? callerUserId = null;
            int? callerMode = null;

            if (!string.IsNullOrEmpty(token))
            {
                var tokenInfo = await _nfcTagService.GetOneLifeTokenAsync(token);
                callerUserId = tokenInfo?.UserId;
                callerMode = tokenInfo?.Mode;
            }

            var view = ServiceRecordView.FromRecord(serviceRecord, callerUserId, callerMode);

            var okBody = ApiResponseFactory.Build(
                HttpStatusCode.OK,
                "Success",
                "Service record retrieved successfully.",
                new Dictionary<string, object> { { "item", view } }
            );

            return new ObjectResult(okBody) { StatusCode = (int)HttpStatusCode.OK };
        }

        if (req.Method == "DELETE")
        {

        }

        if (req.Method == "PUT")
        {
            var formData = await req.ReadFormAsync();
            if (formData == null)
                throw new BadRequestException("Invalid form data");

            var record = await _service.UpdateServiceRecordAsync(id, formData);

            var updatedBody = ApiResponseFactory.Build(
                HttpStatusCode.OK,
                "Successfully Updated",
                "Service record updated successfully.",
                new Dictionary<string, object> { { "item", record! } }
            );

            return new ObjectResult(updatedBody) { StatusCode = (int)HttpStatusCode.OK };
        }

        throw new ApiException(HttpStatusCode.MethodNotAllowed, "Method not allowed");
    }
}

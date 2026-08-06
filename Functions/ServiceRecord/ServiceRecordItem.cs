using System.Net;
using Company.Function;
using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Domain.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace LogMate.Functions.ServiceRecord;

public class ServiceRecordItem(IServiceRecordService serviceRecord, ILogger<ServiceRecordItem> logger)
{
    private readonly IServiceRecordService _service = serviceRecord;
    private readonly ILogger<ServiceRecordItem> _logger = logger;

    [Function("ServiceRecordItem")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get", "delete", "put", "options",
            Route = "servicerecord/{id}")] HttpRequestData req,
        int id)
    {
        if (req.Method == "GET")
        {
            var serviceRecord = await _service.GetById(id);

            if (serviceRecord == null)
                throw new ApiException(HttpStatusCode.NotFound, "Service record not found");

            var okResponse = req.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteAsJsonAsync(new { success = true, data = serviceRecord });
            return okResponse;
        }

        if (req.Method == "DELETE")
        {

        }

        if (req.Method == "PUT")
        {
            if (!req.Headers.TryGetValues("Content-Type", out var values))
                throw new ApiException(HttpStatusCode.BadRequest, "Missing Content-Type header");

            var contentType = MediaTypeHeaderValue.Parse(values.First());
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrEmpty(boundary))
                throw new ApiException(HttpStatusCode.BadRequest, "Missing multipart boundary");

            var reader = new MultipartReader(boundary, req.Body);

            var formFields = new Dictionary<string, string>();
            var uploadedFiles = new List<(string FileName, Stream Content, string ContentType)>();

            MultipartSection? section;
            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                var hasContentDisposition = ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition,
                    out var disposition
                );

                if (!hasContentDisposition || disposition is null)
                    continue;

                if (disposition.IsFileDisposition())
                {
                    uploadedFiles.Add((disposition.FileName.Value!, section.Body, section.ContentType!));
                }
                else if (disposition.IsFormDisposition())
                {
                    using var readerStream = new StreamReader(section.Body);
                    var value = await readerStream.ReadToEndAsync();
                    formFields[disposition.Name.Value!] = value;
                }
            }

            // Map fields
            var record = new Record
            {
                Token = formFields.GetValueOrDefault("Token") ?? string.Empty,
                TagId = formFields.GetValueOrDefault("TagId") ?? string.Empty,
                EnteredDate = formFields.GetValueOrDefault("EnteredDate") ?? string.Empty,
                ServicedDate = formFields.GetValueOrDefault("ServicedDate") ?? string.Empty,
                MechanicName = formFields.GetValueOrDefault("MechanicName") ?? string.Empty,
                Odometer = formFields.GetValueOrDefault("Odometer") ?? string.Empty,
                ServiceCategory = formFields.GetValueOrDefault("ServiceCategory") ?? string.Empty,
                ServiceType = formFields.GetValueOrDefault("ServiceType") ?? string.Empty,
                ServiceOption = formFields.GetValueOrDefault("ServiceOption") ?? string.Empty,
                Comment = formFields.GetValueOrDefault("Comment") ?? string.Empty,
                FileUrls = [],
            };

            // Upload files
            var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg", "image/png", "image/gif", "image/webp", "image/heic", "image/heif",
                "application/pdf",
            };
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp", ".heic", ".heif", ".pdf",
            };

            foreach (var file in uploadedFiles)
            {
                var ext = Path.GetExtension(file.FileName);
                if (!allowedContentTypes.Contains(file.ContentType) || !allowedExtensions.Contains(ext))
                {
                    _logger.LogWarning("Rejected file upload: {FileName} ({ContentType})", file.FileName, file.ContentType);
                    throw new ApiException(HttpStatusCode.UnsupportedMediaType, $"File type not allowed: {file.FileName}. Only images and PDFs are accepted.");
                }

                _logger.LogInformation("Uploading file: {file}", file.FileName);

                string url = await BlobFunctions.UploadBlob(
                    file.Content,
                    file.FileName,
                    file.ContentType
                );

                record.FileUrls.Add(url);
            }

            // Reject expired one-life tokens before writing anything
            var (logId, _) = await SqlFunctions.EnsureOneLifeTokenNotExpiredAsync(record.Token, _logger);

            record.TagId = logId ?? string.Empty;
            record.id = Guid.NewGuid().ToString();

            var result = await CosmosFunctions.InsertRecord(record);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(
                new
                {
                    success = true,
                    message = "Service record updated successfully",
                    data = result,
                }
            );

            return response;
        }

        throw new ApiException(HttpStatusCode.MethodNotAllowed, "Method not allowed");
    }
}

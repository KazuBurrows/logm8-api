using LogMate.Application.Exceptions;
using LogMate.Domain.Models;
using System.Net;
using System.Text.Json.Nodes;
using Company.Function;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

public class SubmitRecord
{
    private readonly ILogger<SubmitRecord> _logger;

    public SubmitRecord(ILogger<SubmitRecord> logger)
    {
        _logger = logger;
    }

    [Function("SubmitRecord")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {            
        req.Headers.TryGetValues("Authorization", out var authValues);
        req.Headers.TryGetValues("X-Tag-Id", out var tagIdValues);
        string? authorization = authValues?.FirstOrDefault();
        string? tagIdHeader = tagIdValues?.FirstOrDefault();

        _logger.LogInformation("Authorization: {Authorization}", authorization);
        _logger.LogInformation("X-Tag-Id: {TagIdHeader}", tagIdHeader);

        if (!req.Headers.TryGetValues("Content-Type", out var values))
            throw new ApiException(HttpStatusCode.BadRequest, "Missing Content-Type header");

        var contentType = MediaTypeHeaderValue.Parse(values.First());
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrEmpty(boundary))
            throw new ApiException(HttpStatusCode.BadRequest, "Missing multipart boundary");

        var reader = new MultipartReader(boundary, req.Body);

        var formFields = new Dictionary<string, string>();
        var uploadedFiles = new List<(string FileName, Stream Content, string ContentType)>();

        MultipartSection section;
        while ((section = await reader.ReadNextSectionAsync()) != null)
        {
            var hasContentDisposition = ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition,
                out var disposition
            );

            if (!hasContentDisposition)
                continue;

            if (disposition.IsFileDisposition())
            {
                uploadedFiles.Add((disposition.FileName.Value, section.Body, section.ContentType));
            }
            else if (disposition.IsFormDisposition())
            {
                using var readerStream = new StreamReader(section.Body);
                var value = await readerStream.ReadToEndAsync();
                formFields[disposition.Name.Value] = value;
            }
        }

        // Map fields
        var record = new Record
        {
            Token = formFields.GetValueOrDefault("Token"),
            TagId = formFields.GetValueOrDefault("TagId"),
            EnteredDate = formFields.GetValueOrDefault("EnteredDate"),
            ServicedDate = formFields.GetValueOrDefault("ServicedDate"),
            MechanicName = formFields.GetValueOrDefault("MechanicName"),
            Odometer = formFields.GetValueOrDefault("Odometer"),
            ServiceCategory = formFields.GetValueOrDefault("ServiceCategory"),
            ServiceType = formFields.GetValueOrDefault("ServiceType"),
            ServiceOption = formFields.GetValueOrDefault("ServiceOption"),

            Comment = formFields.GetValueOrDefault("Comment"),
            FileUrls = new List<string>(),
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

        // Consume token
        string str_log = await SqlFunctions.IsOneLifeUrlConsumed(record.Token);
        var log = JsonNode.Parse(str_log)?.AsObject();

        record.TagId = log?["LogId"]?.ToString();
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
}

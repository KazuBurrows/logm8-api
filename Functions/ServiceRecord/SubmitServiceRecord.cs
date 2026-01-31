using System.Net;
using System.Text.Json;
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
        if (!req.Headers.TryGetValues("Content-Type", out var values))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var contentType = MediaTypeHeaderValue.Parse(values.First());
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrEmpty(boundary))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

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
        var record = new Company.Function.Record
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
        foreach (var file in uploadedFiles)
        {
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
        response.Headers.Add("Content-Type", "application/json");

        await response.WriteStringAsync(JsonSerializer.Serialize(result));

        return response;
    }
}

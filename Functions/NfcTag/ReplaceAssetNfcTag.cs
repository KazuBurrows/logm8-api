using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Common.Http;
using LogMate.Common.Validation;
using LogMate.Domain.Models;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

public class ReplaceAssetNfcTag
{
    private readonly INfcTagService _service;

    public ReplaceAssetNfcTag(INfcTagService service)
    {
        _service = service;
    }

    [Function("ReplaceAssetNfcTag")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var payload = ModelValidator.Validate(JsonConvert.DeserializeObject<ReplaceNfcTagRequest>(body));

        var (status, migratedCount) = await _service.ReplaceAssetNfcTagAsync(payload);

        if (status == HttpStatusCode.NotFound)
            throw new NotFoundException($"No records found for tag {payload.OldTagId}.");

        return await ApiResponseFactory.Ok(req, $"Successfully migrated {migratedCount} record(s).");
    }
}

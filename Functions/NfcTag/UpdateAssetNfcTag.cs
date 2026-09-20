using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using LogMate.Common.Http;
using LogMate.Common.Validation;
using LogMate.Domain.Models;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class UpdateAssetNfcTagAsync
{
    private readonly INfcTagService _service;
    private readonly ILogger<UpdateAssetNfcTagAsync> _logger;

    public UpdateAssetNfcTagAsync(INfcTagService service, ILogger<UpdateAssetNfcTagAsync> logger)
    {
        _service = service;
        _logger = logger;
    }

    [Function("UpdateAssetNfcTagAsync")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req
    )
    {
        req.Headers.TryGetValues("X-Tag-Id", out var tagIdValues);
        string? tagIdHeader = tagIdValues?.FirstOrDefault();

        _logger.LogInformation("X-Tag-Id: {TagIdHeader}", tagIdHeader);

        var body = await req.ReadAsStringAsync();
        var request = ModelValidator.Validate(JsonConvert.DeserializeObject<UpdateAssetNfcTagRequest>(body));

        var success = await _service.UpdateAssetNfcTagAsync(request);

        if (!success)
            throw new NotFoundException("Tag not found or update failed.");

        return await ApiResponseFactory.Ok(req, "Tag updated successfully.");
    }
}
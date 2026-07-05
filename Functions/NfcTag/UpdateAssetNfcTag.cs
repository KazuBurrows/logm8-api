using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
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
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req
    )
    {
        req.Headers.TryGetValues("Authorization", out var authValues);
        req.Headers.TryGetValues("X-Tag-Id", out var tagIdValues);
        string? authorization = authValues?.FirstOrDefault();
        string? tagIdHeader = tagIdValues?.FirstOrDefault();

        _logger.LogInformation("Authorization: {Authorization}", authorization);
        _logger.LogInformation("X-Tag-Id: {TagIdHeader}", tagIdHeader);

        var body = await req.ReadAsStringAsync();
        var request = JsonConvert.DeserializeObject<UpdateAssetNfcTagRequest>(body);

        if (request == null)
            throw new ApiException(HttpStatusCode.BadRequest, "Invalid request body");

        var success = await _service.UpdateAssetNfcTagAsync(request);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(
            new
            {
                success,
                message = success
                    ? "Tag updated successfully"
                    : "Tag not found or update failed",
            }
        );

        return response;
    }
}
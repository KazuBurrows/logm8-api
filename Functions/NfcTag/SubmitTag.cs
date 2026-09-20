using Company.Function;
using LogMate.Application.Exceptions;
using LogMate.Common.Http;
using LogMate.Domain.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class SubmitTag
{
    private readonly ILogger<SubmitTag> _logger;

    public SubmitTag(ILogger<SubmitTag> logger)
    {
        _logger = logger;
    }

    [Function("SubmitTag")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        req.Headers.TryGetValues("X-Tag-Id", out var tagIdValues);
        string? tagIdHeader = tagIdValues?.FirstOrDefault();
        _logger.LogInformation("X-Tag-Id: {TagIdHeader}", tagIdHeader);

        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        string? strTag = query["tag"];

        if (string.IsNullOrEmpty(strTag))
            throw new BadRequestException("Missing tag payload.");

        var tag = JsonConvert.DeserializeObject<Tag>(strTag);
        if (tag == null)
            throw new BadRequestException("Invalid tag payload.");

        bool status = await CosmosFunctions.UpdateTag(tag);

        if (!status)
            throw new NotFoundException("Tag not found or could not be updated.");

        return await ApiResponseFactory.Ok(req, "Tag inserted successfully.");
    }
}

using Company.Function;
using LogMate.Application.Exceptions;
using LogMate.Common.Crypto;
using LogMate.Common.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class AcquireNewTag
{
    private readonly ILogger<AcquireNewTag> _logger;
    private static readonly string Pepper = Environment.GetEnvironmentVariable("HASH_PEPPER")!;

    public AcquireNewTag(ILogger<AcquireNewTag> logger)
    {
        _logger = logger;
    }

    [Function("AcquireNewTag")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        int id = await CosmosFunctions.GetNextTagId();
        if (id == -1)
            throw new NotFoundException("No available tag id.");

        string hashId = PepperHasher.HashWithPepper(id.ToString(), Pepper);

        bool isInserted = await CosmosFunctions.InsertTag(hashId);
        bool isUpdated = await CosmosFunctions.UpdateNextTagId();

        if (!isUpdated)
            throw new NotFoundException("Failed to advance next tag id.");

        if (!isInserted)
            return await ApiResponseFactory.ServerError(req, "Failed to insert new tag.");

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync(hashId);
        return response;
    }
}

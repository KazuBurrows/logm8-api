using Company.Function;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class CosmosKeepAlive
{
    private readonly ILogger<CosmosKeepAlive> _logger;

    public CosmosKeepAlive(ILogger<CosmosKeepAlive> logger)
    {
        _logger = logger;
    }

    // [Function("CosmosKeepAlive")]
    // public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer)
    // {
    //     _logger.LogInformation("CosmosKeepAlive ping starting.");

    //     await CosmosFunctions.GetTagInfo("__warmup__", _logger);
    //     await CosmosFunctions.GetRecordsByTagId("__warmup__", _logger);

    //     _logger.LogInformation("CosmosKeepAlive ping completed.");
    // }
}

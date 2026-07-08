using Company.Function;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class CleanOneLifeUrl
{
    private readonly ILogger<CleanOneLifeUrl> _logger;

    public CleanOneLifeUrl(ILogger<CleanOneLifeUrl> logger)
    {
        _logger = logger;
    }

    [Function("CleanOneLifeUrl")]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("CleanOneLifeUrl ping starting.");

        try
        {
            await SqlFunctions.CleanOneLifeUrls();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cleaning OneLife URLs.");
        }

        _logger.LogInformation("CleanOneLifeUrl ping completed.");
    }
}

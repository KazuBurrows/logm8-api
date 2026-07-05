using System.Net;
using LogMate.Application.Exceptions;
using LogMate.Application.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace LogMate.Application.Services;

public class ErrorHandlerService : IErrorHandlerService
{
    private readonly ILogger<ErrorHandlerService> _logger;

    public ErrorHandlerService(ILogger<ErrorHandlerService> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(FunctionContext context, Exception exception)
    {
        var (statusCode, message) = Map(exception);

        if (exception is ApiException)
        {
            _logger.LogWarning(
                "{FunctionName} returned {StatusCode}: {Message}",
                context.FunctionDefinition.Name,
                (int)statusCode,
                message
            );
        }
        else
        {
            _logger.LogError(
                exception,
                "Unhandled exception in {FunctionName}: {Message}",
                context.FunctionDefinition.Name,
                exception.Message
            );
        }

        var req = await context.GetHttpRequestDataAsync();
        if (req == null)
            return;

        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new { success = false, message });
        context.GetInvocationResult().Value = response;
    }

    private static (HttpStatusCode StatusCode, string Message) Map(Exception exception) =>
        exception switch
        {
            ApiException apiEx => (apiEx.StatusCode, apiEx.Message),
            CosmosException cosmosEx => (cosmosEx.StatusCode, "A database error occurred."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred."),
        };
}

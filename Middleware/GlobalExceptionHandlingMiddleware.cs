using LogMate.Application.Exceptions;
using LogMate.Common.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace LogMate.Middleware;

public class GlobalExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();
        if (request == null)
        {
            await next(context);
            return;
        }

        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            context.GetInvocationResult().Value = await ApiResponseFactory.NotFound(request, ex.Message);
        }
        catch (BadRequestException ex)
        {
            context.GetInvocationResult().Value = await ApiResponseFactory.BadRequest(request, ex.Message);
        }
        catch (ConflictException ex)
        {
            context.GetInvocationResult().Value = await ApiResponseFactory.Conflict(request, ex.Message, ex.ConflictingIds);
        }
        catch (ForbiddenException ex)
        {
            context.GetInvocationResult().Value = await ApiResponseFactory.Forbidden(request, ex.Message);
        }
        catch (UnsupportedMediaTypeException ex)
        {
            context.GetInvocationResult().Value = await ApiResponseFactory.UnsupportedMediaType(request, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in {FunctionName}", context.FunctionDefinition.Name);
            context.GetInvocationResult().Value = await ApiResponseFactory.ServerError(request);
        }
    }
}

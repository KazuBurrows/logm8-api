using LogMate.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace LogMate.Middleware;

public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IErrorHandlerService _errorHandler;

    public ExceptionHandlingMiddleware(IErrorHandlerService errorHandler)
    {
        _errorHandler = errorHandler;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleAsync(context, ex);
        }
    }
}

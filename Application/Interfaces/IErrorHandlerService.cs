using Microsoft.Azure.Functions.Worker;

namespace LogMate.Application.Interfaces;

public interface IErrorHandlerService
{
    Task HandleAsync(FunctionContext context, Exception exception);
}

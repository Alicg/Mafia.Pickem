using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MafiaPickem.Api.Auth;

public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");

            var requestData = await context.GetHttpRequestDataAsync();
            if (requestData != null)
            {
                var response = requestData.CreateResponse(HttpStatusCode.Unauthorized);
                await response.WriteStringAsync(ex.Message);

                var invocationResult = context.GetInvocationResult();
                var outputBindingFeature = context.Features.Get<IInvocationResultFeature>();
                if (outputBindingFeature != null)
                {
                    outputBindingFeature.Result = response;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in function execution");
            throw;
        }
    }
}

public interface IInvocationResultFeature
{
    object? Result { get; set; }
}

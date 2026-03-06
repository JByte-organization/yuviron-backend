using MediatR;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        if (request is ISensitiveRequest)
        {
            _logger.LogInformation("Yuviron Request: Starting {Name} [SENSITIVE DATA HIDDEN]", requestName);
        }
        else
        {
            _logger.LogInformation("Yuviron Request: Starting {Name} {@Request}", requestName, request);
        }

        var response = await next();

        _logger.LogInformation("Yuviron Request: Completed {Name}", requestName);

        return response;
    }
}
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

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

        // 1. Логируем, что запрос начался
        _logger.LogInformation("Yuviron Request: Starting {Name} {@Request}", requestName, request);

        // 2. Передаем управление дальше по цепочке (к валидатору или хендлеру)
        var response = await next();

        // 3. Логируем, что запрос успешно завершился
        _logger.LogInformation("Yuviron Request: Completed {Name}", requestName);

        return response;
    }
}
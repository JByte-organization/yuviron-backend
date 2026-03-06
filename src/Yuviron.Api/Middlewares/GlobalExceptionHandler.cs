using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException && exception is not ArgumentException && exception is not InvalidOperationException && exception is not DomainException)
        {
            _logger.LogError(exception, "System Exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Business Rule Violation: {Message}", exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path
        };

        switch (exception)
        {
            // 1. Ошибки валидации FluentValidation (400)
            case ValidationException validationException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Validation Error";
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                break;

            // 2. Стандартные системные исключения бизнес-логики (400) <-- ДОБАВИЛИ ЭТОТ БЛОК
            case ArgumentException argEx:
            case InvalidOperationException invalidOpEx:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = exception.Message;
                break;

            // 3. Не найдено (404)
            case NotFoundException notFoundEx:
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = notFoundEx.Message;
                break;

            // 4. Конфликты (409)
            case UserAlreadyExistsException existsEx:
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Resource Conflict";
                problemDetails.Detail = existsEx.Message;
                break;

            // 5. Проблемы с доступом (401 / 403)
            case UnauthorizedAccessException unauthorizedEx:
                var isForbidden = unauthorizedEx.Message.StartsWith("Access denied", StringComparison.OrdinalIgnoreCase);

                problemDetails.Status = isForbidden
                    ? StatusCodes.Status403Forbidden
                    : StatusCodes.Status401Unauthorized;
                problemDetails.Title = isForbidden ? "Forbidden" : "Unauthorized";
                problemDetails.Detail = unauthorizedEx.Message;
                break;

            // 6. Базовые доменные ошибки (400)
            case DomainException domainEx:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Business Rule Violation";
                problemDetails.Detail = domainEx.Message;
                break;

            // 7. Реальное падение сервера (500)
            default:
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An unexpected error occurred.";
                break;
        }

        context.Response.StatusCode = problemDetails.Status.Value;
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
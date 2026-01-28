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
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path
        };

        switch (exception)
        {
            // Случай А: Ошибка валидации (то, что выбросил ValidationBehavior)
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

            // Случай Б: кастомный NotFound (Ошибка 404)
            case NotFoundException notFoundEx:
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = notFoundEx.Message; 
                break;

            // Случай В: кастомный DomainException (нарушение бизнес-правил)
            case DomainException domainEx:
                problemDetails.Status = StatusCodes.Status400BadRequest; // Или 422 UnprocessableEntity
                problemDetails.Title = "Business Rule Violation";
                problemDetails.Detail = domainEx.Message;
                break;

            // Случай С: Любая другая ошибка (баг в коде, база упала)
            default:
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An unexpected error occurred.";
                break;
        }

        // 3. Устанавливаем статус код ответа
        context.Response.StatusCode = problemDetails.Status.Value;

        // 4. Отправляем JSON обратно клиенту
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Возвращаем true, сигнализируя, что мы обработали ошибку
        return true;
    }
}
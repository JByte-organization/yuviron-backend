using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

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
        if (exception is not ValidationException && exception is not ArgumentException && exception is not InvalidOperationException && exception is not DomainException && exception is not UnauthorizedAccessException)
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
            // 1. FluentValidation validation errors (400)
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

            // 2. Standard system business logic exceptions (400)
            case ArgumentException argEx:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = argEx.Message;
                break;

            case InvalidOperationException invalidOpEx:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Invalid Operation";
                problemDetails.Detail = invalidOpEx.Message; 
                
                if (invalidOpEx.Message.Contains("image", StringComparison.OrdinalIgnoreCase) || 
                    invalidOpEx.Message.Contains("audio", StringComparison.OrdinalIgnoreCase))
                {
                    problemDetails.Extensions["code"] = "invalid_format"; 
                }
                break;

            // 3. Not found (404)
            case NotFoundException notFoundEx:
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = notFoundEx.Message;
                break;

            // 4. Conflicts (409)
            case UserAlreadyExistsException existsEx:
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Title = "Resource Conflict";
                problemDetails.Detail = existsEx.Message;
                break;

            // 5. Access problems (401 / 403)
            case ForbiddenException forbiddenEx:
                problemDetails.Status = StatusCodes.Status403Forbidden;
                problemDetails.Title = "Forbidden";
                problemDetails.Detail = forbiddenEx.Message;
                break;

            case UnauthorizedAccessException unauthorizedEx:
                var isForbidden = unauthorizedEx.Message.StartsWith("Access denied", StringComparison.OrdinalIgnoreCase);

                problemDetails.Status = isForbidden
                    ? StatusCodes.Status403Forbidden
                    : StatusCodes.Status401Unauthorized;
                problemDetails.Title = isForbidden ? "Forbidden" : "Unauthorized";
                problemDetails.Detail = unauthorizedEx.Message;
                break;

            // 6. Basic domain errors (400)
            case DomainException domainEx:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Business Rule Violation";
                problemDetails.Detail = domainEx.Message;
                break;

            // 7. We catch races and duplicates from the database (409)
            case DbUpdateException dbUpdateEx:
                if (dbUpdateEx.InnerException is MySqlConnector.MySqlException mySqlEx && mySqlEx.Number == 1062)
                {
                    problemDetails.Status = StatusCodes.Status409Conflict;
                    problemDetails.Title = "Resource Conflict";
                    problemDetails.Detail = "A concurrent update or duplicate record was detected. Please try again.";
                }
                else
                {
                    problemDetails.Status = StatusCodes.Status500InternalServerError;
                    problemDetails.Title = "Database Error";
                    problemDetails.Detail = "An error occurred while saving to the database.";
                }
                break;

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
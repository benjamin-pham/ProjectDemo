using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyProject.Application.Exceptions;

namespace MyProject.Infrastructure;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        ExceptionDetails exceptionDetails = GetExceptionDetails(exception);

        ProblemDetails problemDetails = new ProblemDetails
        {
            Status = exceptionDetails.Status,
            Title = exceptionDetails.Title,
            Detail = exceptionDetails.Detail,
            Type = exceptionDetails.Type
        };

        if (exceptionDetails.Errors is not null)
        {
            problemDetails.Extensions["errors"] = exceptionDetails.Errors;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ExceptionDetails GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => new ExceptionDetails(
                Status: StatusCodes.Status400BadRequest,
                Type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title: "ValidationError",
                Detail: "One or more validation errors has occurred",
                Errors: validationException.Errors),
            UnauthorizedAccessException unauthorizedAccessException => new ExceptionDetails(
                Status: StatusCodes.Status401Unauthorized,
                Type: "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title: "Unauthorized",
                Detail: unauthorizedAccessException.Message,
                Errors: null),
            _ => new ExceptionDetails(
                Status: StatusCodes.Status500InternalServerError,
                Type: "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title: "ServerError",
                Detail: "An unexpected error has occurred",
                Errors: null)
        };
    }

    internal record ExceptionDetails(
        int Status,
        string Type,
        string Title,
        string Detail,
        IEnumerable<object>? Errors);
}

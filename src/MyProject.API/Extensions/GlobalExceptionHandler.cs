using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.Exceptions;

namespace MyProject.API.Extensions;

internal sealed class GlobalExceptionHandler(
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
                Type: "ValidationError",
                Title: "Validation Error",
                Detail: "One or more validation errors has occurred",
                Errors: validationException.Errors),
            UnauthorizedAccessException unauthorizedAccessException => new ExceptionDetails(
                Status: StatusCodes.Status401Unauthorized,
                Type: "Unauthorized",
                Title: "Unauthorized",
                Detail: unauthorizedAccessException.Message,
                Errors: null),
            _ => new ExceptionDetails(
                Status: StatusCodes.Status500InternalServerError,
                Type: "ServerError",
                Title: "Server Error",
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

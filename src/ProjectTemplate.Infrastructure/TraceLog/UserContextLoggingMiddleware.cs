using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ProjectTemplate.Domain.Abstractions;
using ProjectTemplate.Infrastructure.Authentication;
using Serilog.Context;

namespace ProjectTemplate.Infrastructure.TraceLog;

internal sealed class UserContextLoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IUserContext userContext)
    {
        var userId = userContext.IsAuthenticated
            ? userContext.UserId.ToString()
            : "unknown";

        using (LogContext.PushProperty("UserId", userId))
        {
            await next(context);
        }
    }
}

public static class UserContextLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseUserContextLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<UserContextLoggingMiddleware>();
}

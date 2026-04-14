using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MyProject.Infrastructure.Authentication;
using Serilog.Context;

namespace MyProject.Infrastructure.TraceLog;

internal sealed class UserContextLoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.GetUserId().ToString()
            : "anonymous";

        context.Items["UserId"] = userId;

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

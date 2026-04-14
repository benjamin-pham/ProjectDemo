using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Serilog.Context;

namespace MyProject.Infrastructure.TraceLog;

internal sealed class CorrelationContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}

public static class CorrelationMiddlewareExtensions
{
    public static IServiceCollection AddCorrelationContextLogging(this IServiceCollection services)
    {
        services.AddTransient<CorrelationIdHandler>();
        services.AddSingleton<IHttpMessageHandlerBuilderFilter, CorrelationIdHandlerBuilderFilter>();
        return services;
    }

    public static IApplicationBuilder UseCorrelationContextLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationContextLoggingMiddleware>();
}

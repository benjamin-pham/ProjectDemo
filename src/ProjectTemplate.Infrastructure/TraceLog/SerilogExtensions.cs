using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ProjectTemplate.Infrastructure.TraceLog;

public static class SerilogExtensions
{
    public static IHostBuilder AddSerilogLogging(this IHostBuilder host)
    {
        host.UseSerilog((context, services, config) =>
        {
            config
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();
        });

        return host;
    }

    public static IApplicationBuilder UseSerilogLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms | CorrelationId: {CorrelationId}";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId) && correlationId is not null)
                    diagnosticContext.Set("CorrelationId", correlationId);
            };
        });
        return app;
    }
}

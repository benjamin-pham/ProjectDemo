using Microsoft.Extensions.Http;
using MyProject.API.Endpoints;
using MyProject.API.Extensions;
using MyProject.Application;
using MyProject.Application.Abstractions.Endpoints;
using MyProject.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.AddSerilogLogging();

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiDocsServices();
builder.Services.AddEndpoints(typeof(IEndpoint).Assembly);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAuthenticationSchemes(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddTransient<CorrelationIdHandler>();
builder.Services.AddSingleton<IHttpMessageHandlerBuilderFilter, CorrelationIdHandlerBuilderFilter>();
// ── Pipeline ─────────────────────────────────────────────────────────────────
var app = builder.Build();
app.MapApiDocsEndpoints();
app.UseExceptionHandler(o => { });
app.UseCorrelationId();
app.UseSerilogLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();
app.MapHealthChecks("/health");

app.MapFallback(async context =>
{
    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("Resource Not Found");
});
app.Run();

// Expose Program for integration tests
public partial class Program { }

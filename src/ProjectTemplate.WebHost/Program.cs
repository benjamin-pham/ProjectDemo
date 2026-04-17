using System.Text.Json.Serialization;
using ProjectTemplate.Application;
using ProjectTemplate.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseInfrastructureServices();
app.MapHealthChecks("/health");
app.MapFallback(async context =>
{
    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("Resource Not Found");
});

app.Run();

// Expose Program for integration tests
public partial class Program { }

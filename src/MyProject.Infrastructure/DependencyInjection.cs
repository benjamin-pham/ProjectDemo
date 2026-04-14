using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyProject.Application.Abstractions.Authentication;
using MyProject.Application.Abstractions.Data;
using MyProject.Application.Abstractions.Endpoints;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Repositories;
using MyProject.Infrastructure.ApiDocs;
using MyProject.Infrastructure.Authentication;
using MyProject.Infrastructure.Caching;
using MyProject.Infrastructure.Clock;
using MyProject.Infrastructure.Data;
using MyProject.Infrastructure.Endpoints;
using MyProject.Infrastructure.Repositories;
using MyProject.Infrastructure.TraceLog;

namespace MyProject.Infrastructure;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        builder.Host.AddSerilogLogging();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddApiDocsServices();
        builder.Services.AddEndpoints(typeof(IEndpoint).Assembly);

        var connectionString = builder.Configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        builder.Services.AddSingleton<ISqlConnectionFactory>(
            _ => new SqlConnectionFactory(connectionString));

        builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserContext, UserContext>();

        builder.Services.AddCachingServices();

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
        builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
        builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

        builder.Services.AddAuthenticationSchemes(builder.Configuration);
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddCorrelationContextLogging();

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IRoleRepository, RoleRepository>();

        return builder;
    }

    public static WebApplication UseInfrastructureServices(this WebApplication app)
    {
        app.UseExceptionHandler(o => { });

        app.UseCorrelationContextLogging();

        app.UseSerilogLogging();

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseUserContextLogging();

        app.UseAuthorization();

        app.MapApiDocsEndpoints();

        app.MapEndpoints();

        return app;
    }
}

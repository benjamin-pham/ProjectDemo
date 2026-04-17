using Scalar.AspNetCore;
using Microsoft.OpenApi;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectTemplate.Infrastructure.ApiDocs;

public static class ApiDocsExtensions
{
    public static IServiceCollection AddApiDocsServices(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in assemblies)
            {
                var xmlPath = Path.ChangeExtension(asm.Location, ".xml");
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }

            options.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "Please insert ApiKey into field",
                Name = "X-Api-Key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("BearerAuth"),
                    new List<string>()
                },
                {
                    new OpenApiSecuritySchemeReference("ApiKey"),
                    new List<string>()
                }
            });

            options.OperationFilter<ProblemDetailsOperationFilter>();

            options.SchemaFilter<EnumSchemaFilter>();

        });
        services.AddFluentValidationRulesToSwagger();
        return services;
    }

    public static IApplicationBuilder MapApiDocsEndpoints(this WebApplication app)
    {
        app.MapSwagger("/openapi/{documentName}.json");
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("ProjectTemplate API")
                .ShowOperationId()
                .SortTagsAlphabetically()
                .SortOperationsByMethod()
                .WithTheme(ScalarTheme.Moon)
                .PreserveSchemaPropertyOrder()
                .DisableAgent()
                .AddPreferredSecuritySchemes("BearerAuth", "ApiKey")
                .AddHttpAuthentication("BearerAuth", auth =>
                {
                    auth.Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
                })
                .AddApiKeyAuthentication("ApiKey", apiKey =>
                {
                    apiKey.Value = "sk-dev-api-key-12345";
                });
        }).RequireAuthorization("ScalarBasic");

        return app;
    }
}

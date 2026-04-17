using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjectTemplate.Infrastructure.ApiDocs;

public class ProblemDetailsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses?.TryAdd("400", new OpenApiResponse
        {
            Description = "Bad Request",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(
                        typeof(ProblemDetails),
                        context.SchemaRepository)
                }
            }
        });

        operation.Responses?.TryAdd("500", new OpenApiResponse
        {
            Description = "Internal Server Error",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(
                        typeof(ProblemDetails),
                        context.SchemaRepository)
                }
            }
        });
    }
}
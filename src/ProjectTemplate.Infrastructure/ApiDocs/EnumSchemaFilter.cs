using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProjectTemplate.Infrastructure.ApiDocs;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
            return;

        if (schema is not OpenApiSchema concreteSchema)
            return;

        concreteSchema.Enum?.Clear();
        concreteSchema.Type = JsonSchemaType.String;
        concreteSchema.Format = null;

        concreteSchema.Enum = [.. Enum.GetNames(context.Type).Select(name => (JsonNode)JsonValue.Create(name)!)];
    }
}
using ECommerceStoreUsers.API.Configuration.Common;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace ECommerceStoreUsers.API.Configuration;

/// <summary>Documents serializer and problem-writer guarantees that CLR annotations alone cannot describe.</summary>
public sealed class UsersContractSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concrete)
            return;
        if (context.MemberInfo is null && context.Type.Namespace?.StartsWith("ECommerceStoreUsers.Application.Common.RequestsDto", StringComparison.Ordinal) == true)
        {
            // System.Text.Json's web defaults ignore unknown members; this is not a closed request object.
            concrete.AdditionalPropertiesAllowed = true;
            concrete.AdditionalProperties = null;
        }
        if (context.Type != typeof(ApiProblemDetails) || concrete.Properties is null)
            return;

        // ApiProblemResponse sets all nine fields, including non-null inherited ProblemDetails members.
        concrete.Required = concrete.Properties.Keys.ToHashSet(StringComparer.Ordinal);
        foreach (var property in concrete.Properties.Values.OfType<OpenApiSchema>())
            property.Type &= ~JsonSchemaType.Null;
        foreach (var (name, description, example) in new[]
        {
            ("type", "URI identifying the HTTP problem category.", "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),
            ("title", "Safe human-readable problem title.", "Validation failed."),
            ("detail", "Safe explanation of this failure without internal exception details.", "Validation failed."),
            ("instance", "Request path that produced this problem.", "/admins")
        })
        {
            concrete.Properties[name].Description = description;
            if (concrete.Properties[name] is OpenApiSchema property)
                property.Example = JsonValue.Create(example);
        }
        concrete.Properties["status"].Description = "HTTP status code, equal to the response status.";
        if (concrete.Properties["status"] is OpenApiSchema status)
            status.Example = JsonValue.Create(400);
    }
}

using ECommerceStoreUsers.API.Configuration.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Domain.Validation.Common;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

/// <summary>Compares the generated contract with real serializer/type metadata, never a second schema file.</summary>
internal static class OpenApiContractAssertions
{
    internal static object Check(JsonDocument document)
    {
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        var nullability = new NullabilityInfoContext();
        var types = new[] { typeof(ApiProblemDetails).Assembly, typeof(CreateCustomerRequestDto).Assembly, typeof(ValidationError).Assembly }
            .SelectMany(assembly => assembly.GetTypes()).ToDictionary(type => type.FullName!, type => type);
        var checkedProperties = new List<object>();
        foreach (var entry in schemas.EnumerateObject())
        {
            var type = types.Values.Single(type => type.Name == entry.Name);
            Require(entry.Value.TryGetProperty("description", out var description) && !string.IsNullOrWhiteSpace(description.GetString()), $"Missing type description: {entry.Name}");
            var metadata = options.GetTypeInfo(type).Properties.Where(property => !property.IsExtensionData).ToArray();
            var properties = entry.Value.GetProperty("properties");
            Require(properties.EnumerateObject().Select(property => property.Name).Order().SequenceEqual(metadata.Select(property => property.Name).Order()), $"Serializer member drift: {entry.Name}");
            var expectedRequired = metadata.Where(property => type == typeof(ApiProblemDetails) || property.IsRequired).Select(property => property.Name).Order();
            IEnumerable<string> required = entry.Value.TryGetProperty("required", out var fields) ? fields.EnumerateArray().Select(field => field.GetString()!).Order() : [];
            Require(required.SequenceEqual(expectedRequired), $"Required member drift: {entry.Name}");
            foreach (var property in metadata)
            {
                var schema = properties.GetProperty(property.Name);
                var member = (PropertyInfo)property.AttributeProvider!;
                var nullable = type != typeof(ApiProblemDetails) && nullability.Create(member).ReadState == NullabilityState.Nullable;
                Require((schema.TryGetProperty("nullable", out var allowsNull) && allowsNull.GetBoolean()) == nullable, $"Nullability drift: {entry.Name}.{property.Name}");
                CheckType(schema, property.PropertyType, $"{entry.Name}.{property.Name}");
                Require(schema.TryGetProperty("description", out var fieldDescription) && !string.IsNullOrWhiteSpace(fieldDescription.GetString()), $"Missing field description: {entry.Name}.{property.Name}");
                Require(schema.TryGetProperty("example", out var example), $"Missing typed example: {entry.Name}.{property.Name}");
                var errors = JsonContractValidator.Check(schema, example, schemas);
                Require(errors.Count == 0, $"Invalid typed example: {entry.Name}.{property.Name}: {string.Join("; ", errors)}");
                checkedProperties.Add(new { schema = entry.Name, field = property.Name, required = property.IsRequired || type == typeof(ApiProblemDetails), nullable, example = example.Clone() });
            }
            if (type.Namespace?.Contains(".RequestsDto", StringComparison.Ordinal) == true)
                Require(!entry.Value.TryGetProperty("additionalProperties", out var extra) || extra.ValueKind != JsonValueKind.False, $"Request schema incorrectly closes ignored members: {entry.Name}");
        }
        var parameterCount = 0;
        foreach (var path in document.RootElement.GetProperty("paths").EnumerateObject())
            foreach (var operation in path.Value.EnumerateObject())
                if (operation.Value.TryGetProperty("parameters", out var parameters))
                    foreach (var parameter in parameters.EnumerateArray())
                    {
                        Require(parameter.TryGetProperty("description", out var description) && !string.IsNullOrWhiteSpace(description.GetString()), $"Missing route parameter description: {path.Name}");
                        Require(parameter.TryGetProperty("example", out var example), $"Missing route parameter example: {path.Name}");
                        Require(JsonContractValidator.Check(parameter.GetProperty("schema"), example, schemas).Count == 0, $"Invalid route parameter example: {path.Name}");
                        parameterCount++;
                    }
        return new { parameters = parameterCount, schemas = schemas.EnumerateObject().Count(), properties = checkedProperties.Count, checkedProperties };

        void CheckType(JsonElement schema, Type type, string path)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (schema.TryGetProperty("allOf", out var allOf))
            {
                Require(allOf.GetArrayLength() == 1, $"Unexpected composition at {path}");
                CheckType(allOf[0], type, path);
                return;
            }
            if (schema.TryGetProperty("$ref", out var reference))
            {
                Require(reference.GetString() == $"#/components/schemas/{type.Name}", $"Reference type drift at {path}");
                return;
            }
            var expected = type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) ? "string"
                : type == typeof(bool) ? "boolean" : type == typeof(int) || type == typeof(long) ? "integer"
                : typeof(IDictionary).IsAssignableFrom(type) || type.GetInterfaces().Any(item => item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IDictionary<,>)) ? "object"
                : typeof(IEnumerable).IsAssignableFrom(type) ? "array" : "object";
            Require(schema.GetProperty("type").GetString() == expected, $"Scalar/container type drift at {path}");
            var format = type == typeof(Guid) ? "uuid" : type == typeof(DateTime) ? "date-time" : type == typeof(int) ? "int32" : type == typeof(long) ? "int64" : null;
            if (format is not null)
                Require(schema.TryGetProperty("format", out var actualFormat) && actualFormat.GetString() == format, $"Format drift at {path}");
            if (expected == "array")
                CheckType(schema.GetProperty("items"), type.IsArray ? type.GetElementType()! : type.GenericTypeArguments.Single(), path + "[]");
            if (expected == "object" && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                CheckType(schema.GetProperty("additionalProperties"), type.GenericTypeArguments[1], path + ".*");
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}

using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

/// <summary>Checks real acceptance HTTP responses against the generated Swagger document.</summary>
internal sealed class OpenApiResponseHandler(JsonDocument document) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
            return response;

        var paths = document.RootElement.GetProperty("paths");
        var route = paths.EnumerateObject()
            .Where(item => Matches(item.Name, path))
            .OrderByDescending(item => item.Name.Split('/').Count(part => !part.StartsWith('{')))
            .FirstOrDefault(item => item.Value.TryGetProperty(request.Method.Method.ToLowerInvariant(), out _));
        if (route.Value.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException($"HTTP operation absent from OpenAPI: {request.Method} {path}");

        var operation = route.Value.GetProperty(request.Method.Method.ToLowerInvariant());
        var status = ((int)response.StatusCode).ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (!operation.GetProperty("responses").TryGetProperty(status, out var declared))
            throw new InvalidOperationException($"Undeclared runtime status: {operation.GetProperty("operationId").GetString()} {status}");

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        if ((int)response.StatusCode == 204)
        {
            if (bytes.Length != 0 || declared.TryGetProperty("content", out _))
                throw new InvalidOperationException($"204 response contains or declares a body: {path}");
            return response;
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType is null || !declared.TryGetProperty("content", out var content) ||
            !content.TryGetProperty(mediaType, out var representation) ||
            !representation.TryGetProperty("schema", out var schema) || bytes.Length == 0)
            throw new InvalidOperationException($"Response media type/body differs from OpenAPI: {request.Method} {path} {status} ({mediaType})");

        var expectedMedia = (int)response.StatusCode >= 400 ? "application/problem+json" : "application/json";
        if (mediaType != expectedMedia)
            throw new InvalidOperationException($"Unexpected response media type: {request.Method} {path} {status} ({mediaType})");

        using var body = JsonDocument.Parse(bytes);
        Validate(schema, body.RootElement, document.RootElement.GetProperty("components").GetProperty("schemas"), path, 0);
        return response;
    }

    private static bool Matches(string template, string path)
    {
        var expected = template.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var actual = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return expected.Length == actual.Length && expected.Zip(actual).All(parts =>
            parts.First.StartsWith('{') && parts.First.EndsWith('}') ||
            string.Equals(parts.First, parts.Second, StringComparison.OrdinalIgnoreCase));
    }

    private static void Validate(JsonElement schema, JsonElement value, JsonElement components, string path, int depth)
    {
        if (depth > 16)
            return; // Recursive DTOs can be arbitrarily deep; validate the represented root and children.
        if (schema.TryGetProperty("$ref", out var reference))
        {
            var name = reference.GetString()!.Split('/').Last();
            Validate(components.GetProperty(name), value, components, path, depth + 1);
            return;
        }
        if (schema.TryGetProperty("allOf", out var allOf))
        {
            foreach (var part in allOf.EnumerateArray())
                Validate(part, value, components, path, depth + 1);
        }
        if (!schema.TryGetProperty("type", out var type))
            return;
        if (type.GetString() == "array")
        {
            if (value.ValueKind != JsonValueKind.Array)
                throw new InvalidOperationException($"Expected array response at {path}");
            if (schema.TryGetProperty("items", out var items))
                foreach (var element in value.EnumerateArray())
                    Validate(items, element, components, path, depth + 1);
        }
        if (type.GetString() == "object")
        {
            if (value.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException($"Expected object response at {path}");
            if (schema.TryGetProperty("required", out var required))
                foreach (var field in required.EnumerateArray())
                    if (!value.TryGetProperty(field.GetString()!, out _))
                        throw new InvalidOperationException($"Required response field missing at {path}: {field.GetString()}");
            if (schema.TryGetProperty("properties", out var properties))
                foreach (var property in properties.EnumerateObject())
                    if (value.TryGetProperty(property.Name, out var child) && child.ValueKind != JsonValueKind.Null)
                        Validate(property.Value, child, components, path, depth + 1);
        }
    }
}

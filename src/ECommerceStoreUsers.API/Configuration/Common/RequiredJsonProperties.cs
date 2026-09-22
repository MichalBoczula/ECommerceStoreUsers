using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http.Metadata;

namespace ECommerceStoreUsers.API.Configuration.Common;

internal static class RequiredJsonProperties
{
    private const long MaxInspectableBytes = 64 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    public static void EnableInspection(HttpContext context)
    {
        if (context.Request.ContentLength is > 0 and <= MaxInspectableBytes &&
            context.Request.HasJsonContentType())
        {
            context.Request.EnableBuffering();
        }
    }

    public static async Task<IReadOnlyCollection<string>> FindMissingAsync(
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var requestType = context.GetEndpoint()?.Metadata.GetMetadata<IAcceptsMetadata>()?.RequestType;
        if (requestType is null || !context.Request.Body.CanSeek ||
            context.Request.Body.Length > MaxInspectableBytes)
        {
            return [];
        }

        var originalPosition = context.Request.Body.Position;
        try
        {
            context.Request.Body.Position = 0;
            using var document = await JsonDocument.ParseAsync(context.Request.Body, cancellationToken: cancellationToken);
            var missing = new HashSet<string>(StringComparer.Ordinal);
            CollectMissing(document.RootElement, JsonOptions.GetTypeInfo(requestType), missing);
            return missing.ToArray();
        }
        catch (JsonException)
        {
            // Malformed JSON has no reliable set of missing members.
            return [];
        }
        finally
        {
            context.Request.Body.Position = originalPosition;
        }
    }

    private static void CollectMissing(JsonElement element, JsonTypeInfo type, HashSet<string> missing)
    {
        if (element.ValueKind != JsonValueKind.Object || type.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        foreach (var property in type.Properties)
        {
            var found = element.EnumerateObject().FirstOrDefault(
                value => string.Equals(value.Name, property.Name, StringComparison.OrdinalIgnoreCase));

            if (found.Name is null)
            {
                if (property.IsRequired)
                {
                    missing.Add(property.Name);
                }

                continue;
            }

            if (found.Value.ValueKind == JsonValueKind.Object)
            {
                CollectMissing(found.Value, JsonOptions.GetTypeInfo(property.PropertyType), missing);
            }
        }
    }
}

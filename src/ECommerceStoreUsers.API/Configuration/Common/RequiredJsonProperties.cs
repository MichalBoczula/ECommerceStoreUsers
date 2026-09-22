using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http.Metadata;

namespace ECommerceStoreUsers.API.Configuration.Common;

internal static class RequiredJsonProperties
{
    private const long MaxInspectableBytes = 64 * 1024;
    private const string RequestTypeKey = "Ref06.JsonRequestType";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    public static void EnableInspection(HttpContext context)
    {
        context.Items[RequestTypeKey] =
            context.GetEndpoint()?.Metadata.GetMetadata<IAcceptsMetadata>()?.RequestType;

        if (context.Request.HasJsonContentType() &&
            (context.Request.ContentLength is null or <= MaxInspectableBytes))
        {
            context.Request.EnableBuffering();
        }
    }

    public static async Task<MissingJsonFields> FindMissingAsync(
        HttpContext context,
        CancellationToken cancellationToken)
    {
        context.Items.TryGetValue(RequestTypeKey, out var requestTypeValue);
        var requestType = requestTypeValue as Type;
        if (requestType is null || !context.Request.Body.CanSeek ||
            context.Request.Body.Length > MaxInspectableBytes)
        {
            return new MissingJsonFields(string.Empty, []);
        }

        var originalPosition = context.Request.Body.Position;
        try
        {
            context.Request.Body.Position = 0;
            using var document = await JsonDocument.ParseAsync(context.Request.Body, cancellationToken: cancellationToken);
            return CollectMissing(document.RootElement, JsonOptions.GetTypeInfo(requestType));
        }
        catch (JsonException)
        {
            // Malformed JSON has no reliable set of missing members.
            return new MissingJsonFields(string.Empty, []);
        }
        catch (Exception exception) when (exception is NotSupportedException or InvalidOperationException or IOException)
        {
            // Metadata or buffering failures must never replace the original request error.
            return new MissingJsonFields(string.Empty, []);
        }
        finally
        {
            context.Request.Body.Position = originalPosition;
        }
    }

    private static MissingJsonFields CollectMissing(JsonElement element, JsonTypeInfo type)
    {
        if (element.ValueKind != JsonValueKind.Object || type.Kind != JsonTypeInfoKind.Object)
        {
            return new MissingJsonFields(string.Empty, []);
        }

        var missing = new List<string>();
        foreach (var property in type.Properties)
        {
            var found = false;
            JsonElement value = default;
            foreach (var member in element.EnumerateObject())
            {
                if (!string.Equals(member.Name, property.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                found = true;
                value = member.Value;
                break;
            }

            if (!found)
            {
                if (property.IsRequired)
                {
                    missing.Add(property.Name);
                }

                continue;
            }

            if (value.ValueKind == JsonValueKind.Object)
            {
                var nested = CollectMissing(value, JsonOptions.GetTypeInfo(property.PropertyType));
                if (nested.Names.Count > 0)
                {
                    return nested;
                }
            }
        }

        return new MissingJsonFields(type.Type.Name, missing);
    }
}

internal sealed record MissingJsonFields(string TypeName, IReadOnlyCollection<string> Names);

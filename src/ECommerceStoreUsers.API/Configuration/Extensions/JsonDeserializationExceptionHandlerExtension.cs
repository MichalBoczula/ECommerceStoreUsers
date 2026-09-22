using ECommerceStoreUsers.API.Configuration.Common;
using Microsoft.AspNetCore.Http.Metadata;

namespace ECommerceStoreUsers.API.Configuration.Extensions;

public static class JsonDeserializationExceptionHandlerExtension
{
    public static async Task HandleJsonDeserializationException(
        this HttpContext context,
        CancellationToken cancellationToken)
    {
        var missing = await RequiredJsonProperties.FindMissingAsync(context, cancellationToken);
        var requestType = context.GetEndpoint()?.Metadata.GetMetadata<IAcceptsMetadata>()?.RequestType;
        var detail = missing.Count > 0
            ? $"JSON payload for {requestType?.Name ?? "request"} is missing required properties: {string.Join(", ", missing)}."
            : "The request body is not valid JSON.";

        await ApiProblemResponse.WriteAsync(
            context,
            StatusCodes.Status400BadRequest,
            "invalid_json",
            "Invalid JSON payload.",
            detail,
            cancellationToken,
            missingProperties: missing);
    }
}

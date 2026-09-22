using ECommerceStoreUsers.API.Configuration.Common;

namespace ECommerceStoreUsers.API.Configuration.Extensions;

public static class JsonDeserializationExceptionHandlerExtension
{
    public static async Task HandleJsonDeserializationException(
        this HttpContext context,
        CancellationToken cancellationToken)
    {
        var missing = await RequiredJsonProperties.FindMissingAsync(context, cancellationToken);
        var detail = missing.Names.Count > 0
            ? $"JSON payload for {missing.TypeName} is missing required properties: {string.Join(", ", missing.Names)}."
            : "The request body is not valid JSON.";

        await ApiProblemResponse.WriteAsync(
            context,
            StatusCodes.Status400BadRequest,
            "invalid_json",
            "Invalid JSON payload.",
            detail,
            cancellationToken,
            missingProperties: missing.Names);
    }
}

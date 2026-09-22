using ECommerceStoreUsers.API.Configuration.Common;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.API.Configuration.Extensions
{
    public static class ResourceAlreadyExistsExceptionHandlerExtension
    {
        public static async Task HandleResourceAlreadyExistsException(
            this HttpContext context,
            ResourceAlreadyExistsException exception,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            logger.LogWarning(
                "Resource conflict detected: Type {ResourceType} with Id {ResourceId} already exists during action {ActionName} at path {RequestPath}. TraceId: {TraceId}",
                exception.ResourceType,
                exception.ResourceId,
                exception.ActionName,
                context.Request.Path,
                context.TraceIdentifier);

            await ApiProblemResponse.WriteAsync(
                context,
                StatusCodes.Status409Conflict,
                "resource_conflict",
                "Conflict.",
                $"Resource {exception.ResourceType} identified by id {exception.ResourceId} already exists in db. Error in action {exception.ActionName}.",
                cancellationToken);
        }
    }
}

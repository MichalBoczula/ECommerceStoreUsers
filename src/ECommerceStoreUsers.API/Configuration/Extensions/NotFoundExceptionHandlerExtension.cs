using ECommerceStoreUsers.API.Configuration.Common;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.API.Configuration.Extensions
{
    public static class NotFoundExceptionHandlerExtension
    {
        public static async Task HandleNotFoundException(
            this HttpContext context,
            ResourceNotFoundException exception,
            CancellationToken cancellationToken)
        {
            await ApiProblemResponse.WriteAsync(
                context,
                StatusCodes.Status404NotFound,
                "resource_not_found",
                "Resource not found.",
                $"Resource {exception.ResourceType} identified by id {exception.ResourceId} cannot be found in database during action {exception.ActionName}.",
                cancellationToken);
        }
    }
}

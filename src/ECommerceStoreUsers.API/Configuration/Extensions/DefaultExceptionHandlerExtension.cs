using ECommerceStoreUsers.API.Configuration.Common;

namespace ECommerceStoreUsers.API.Configuration.Extensions
{
    internal static class DefaultExceptionHandlerExtension
    {
        public static async Task HandleDefaultException(
            this HttpContext context,
            Exception exception,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {RequestMethod} {RequestPath}. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await ApiProblemResponse.WriteAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "internal_error",
                "Server error.",
                "An unexpected error occurred.",
                cancellationToken);
        }
    }
}

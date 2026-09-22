using ECommerceStoreInvoice.API.Configuration.Common;

namespace ECommerceStoreInvoice.API.Configuration.Extensions
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

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(
                new InternalServerErrorProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server error.",
                    Detail = "An unexpected error occurred.",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                    Instance = context.Request.Path,
                    TraceId = context.TraceIdentifier
                },
                options: null,
                contentType: "application/problem+json",
                cancellationToken);
        }
    }
}

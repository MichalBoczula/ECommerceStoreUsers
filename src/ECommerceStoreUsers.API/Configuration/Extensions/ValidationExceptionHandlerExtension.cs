using ECommerceStoreUsers.API.Configuration.Common;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.API.Configuration.Extensions
{
    public static class ValidationExceptionHandlerExtension
    {
        public static async Task HandleValidationException(
            this HttpContext context,
            ValidationException validationException,
            CancellationToken cancellationToken)
        {
            await ApiProblemResponse.WriteAsync(
                context,
                StatusCodes.Status400BadRequest,
                "validation_failed",
                "Validation failed.",
                "One or more validation errors occurred.",
                cancellationToken,
                errors: validationException.ValidationResult.GetValidationErrors());
        }
    }
}

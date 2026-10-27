using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.API.Configuration.Common;
using ECommerceStoreUsers.API.Configuration.Extensions;
using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace ECommerceStoreUsers.API.Configuration
{
    [FlowOutcome("users.branch.http.invalid-json", "JSON parsing, conversion or required-member binding fails.", "Return the safe invalid_json problem and any inspected missing member names; do not claim a business policy ran.", Status = 400, Code = "invalid_json", Stage = "framework")]
    [FlowOutcome("users.branch.http.invalid-request", "Body binding rejects a missing/empty/root-null request without a JSON conversion error.", "Return the complete safe invalid_request problem; no endpoint write is performed.", Status = 400, Code = "invalid_request", Stage = "framework")]
    [FlowOutcome("users.branch.http.internal-error", "An unhandled dependency/provider/persistence failure reaches the exception handler.", "Return a generic safe 500 without exception internals. Locate write/rollback scope in that scenario's persistence evidence, not the status alone.", Status = 500, Code = "internal_error", Stage = "exception-handler")]
    public sealed class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            await (exception switch
            {
                ValidationException validationException =>
                    ValidationExceptionHandlerExtension.HandleValidationException(
                        context, validationException, cancellationToken),

                ResourceNotFoundException notFoundException =>
                    NotFoundExceptionHandlerExtension.HandleNotFoundException(
                        context, notFoundException, cancellationToken),

                ResourceAlreadyExistsException resourceAlreadyExistsException =>
                    ResourceAlreadyExistsExceptionHandlerExtension.HandleResourceAlreadyExistsException(
                        context, resourceAlreadyExistsException, _logger, cancellationToken),

                ConcurrencyConflictException => ApiProblemResponse.WriteAsync(
                    context, StatusCodes.Status409Conflict, "concurrency_conflict", "Conflict.",
                    "The resource was changed by another request. Reload it and retry.", cancellationToken),

                BadHttpRequestException badHttpRequestException when badHttpRequestException.InnerException is JsonException =>
                    JsonDeserializationExceptionHandlerExtension.HandleJsonDeserializationException(
                        context, cancellationToken),

                JsonException jsonException =>
                    JsonDeserializationExceptionHandlerExtension.HandleJsonDeserializationException(
                        context, cancellationToken),

                BadHttpRequestException badHttpRequestException =>
                    ApiProblemResponse.WriteAsync(
                        context,
                        badHttpRequestException.StatusCode == StatusCodes.Status415UnsupportedMediaType
                            ? StatusCodes.Status415UnsupportedMediaType
                            : StatusCodes.Status400BadRequest,
                        badHttpRequestException.StatusCode == StatusCodes.Status415UnsupportedMediaType
                            ? "unsupported_media_type" : "invalid_request",
                        "Invalid request.",
                        "The request could not be processed.",
                        cancellationToken),

                _ => DefaultExceptionHandlerExtension.HandleDefaultException(context, exception, _logger, cancellationToken)
            });

            return true;
        }
    }
}


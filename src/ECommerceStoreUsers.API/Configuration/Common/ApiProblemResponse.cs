using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.API.Configuration.Common;

[FlowOutcome("users.branch.http.route-not-found", "No route matches, including a rejected GUID route constraint.", "Return complete route_not_found problem JSON with no business operation assumed.", Status = 404, Code = "route_not_found", Stage = "framework")]
[FlowOutcome("users.branch.http.method-not-allowed", "The route exists but does not allow the method.", "Return the safe method_not_allowed problem and the router's actual Allow header.", Status = 405, Code = "method_not_allowed", Stage = "framework")]
[FlowOutcome("users.branch.http.unsupported-media-type", "A nonempty body has an unsupported or absent media type.", "Return complete unsupported_media_type problem JSON before the endpoint write.", Status = 415, Code = "unsupported_media_type", Stage = "framework")]
internal static class ApiProblemResponse
{
    public static Task WriteAsync(
        HttpContext context,
        int status,
        string code,
        string title,
        string detail,
        CancellationToken cancellationToken,
        IEnumerable<ValidationError>? errors = null,
        IReadOnlyCollection<string>? missingProperties = null)
    {
        context.Response.StatusCode = status;

        return context.Response.WriteAsJsonAsync(
            new ApiProblemDetails
            {
                Status = status,
                Type = status switch
                {
                    400 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    404 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
                    405 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.5",
                    409 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
                    415 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.13",
                    500 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                    503 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4",
                    _ => "about:blank"
                },
                Title = title,
                Detail = detail,
                Instance = context.Request.Path,
                Code = code,
                TraceId = context.TraceIdentifier,
                Errors = errors ?? [],
                MissingProperties = missingProperties ?? []
            },
            options: null,
            contentType: "application/problem+json",
            cancellationToken);
    }

    public static Task WriteEmptyStatusAsync(HttpContext context)
    {
        var (code, title, detail) = context.Response.StatusCode switch
        {
            404 when context.GetEndpoint() is not null =>
                ("resource_not_found", "Resource not found.", "The requested resource does not exist."),
            404 => ("route_not_found", "Route not found.", "The requested route does not exist."),
            405 => ("method_not_allowed", "Method not allowed.", "The requested method is not supported by this route."),
            415 => ("unsupported_media_type", "Unsupported media type.", "The request content type is not supported."),
            503 => ("service_unavailable", "Service unavailable.", "The service is temporarily unavailable."),
            _ => ($"http_{context.Response.StatusCode}", "Request failed.", "The request could not be processed.")
        };

        return WriteAsync(context, context.Response.StatusCode, code, title, detail, context.RequestAborted);
    }
}


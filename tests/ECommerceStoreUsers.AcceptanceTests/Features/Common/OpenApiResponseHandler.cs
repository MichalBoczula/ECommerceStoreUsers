using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

/// <summary>Checks actual acceptance requests/responses against the generated Swagger document.</summary>
internal sealed class OpenApiResponseHandler(JsonDocument document, Action<string, object>? record = null) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        try
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (path is "/health" or "/health/live" or "/health/ready" || path.StartsWith("/swagger/", StringComparison.OrdinalIgnoreCase))
                return response;

            var paths = document.RootElement.GetProperty("paths");
            var route = paths.EnumerateObject()
                .Where(item => Matches(item.Name, path))
                .OrderByDescending(item => item.Name.Split('/').Count(part => !part.StartsWith('{')))
                .FirstOrDefault(item => item.Value.TryGetProperty(request.Method.Method.ToLowerInvariant(), out var candidate) &&
                    MatchesPathParameters(item.Name, path, candidate));
            if (route.Value.ValueKind == JsonValueKind.Undefined)
            {
                if ((int)response.StatusCode is 404 or 405)
                {
                    await ValidateFrameworkProblem(request, response, cancellationToken);
                    return response;
                }
                throw new InvalidOperationException($"HTTP operation absent from OpenAPI: {request.Method} {path}");
            }

            var operation = route.Value.GetProperty(request.Method.Method.ToLowerInvariant());
            var status = (int)response.StatusCode;
            if (!operation.GetProperty("responses").TryGetProperty(status.ToString(System.Globalization.CultureInfo.InvariantCulture), out var declared))
            {
                if (status == 415)
                {
                    await ValidateFrameworkProblem(request, response, cancellationToken);
                    return response;
                }
                throw new InvalidOperationException($"Undeclared runtime status: {operation.GetProperty("operationId").GetString()} {status}");
            }

            var components = document.RootElement.GetProperty("components").GetProperty("schemas");
            var requestErrors = new List<string>();
            var parameterCount = 0;
            if (operation.TryGetProperty("parameters", out var parameters))
            {
                var template = route.Name.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                foreach (var parameter in parameters.EnumerateArray())
                {
                    if (parameter.GetProperty("in").GetString() != "path")
                        throw new InvalidOperationException("Unsupported parameter location in the Users contract checker.");
                    var index = Array.IndexOf(template, "{" + parameter.GetProperty("name").GetString() + "}");
                    var value = JsonSerializer.SerializeToElement(Uri.UnescapeDataString(segments[index]));
                    requestErrors.AddRange(JsonContractValidator.Check(parameter.GetProperty("schema"), value, components));
                    parameterCount++;
                }
            }
            if (operation.TryGetProperty("requestBody", out var requestBody))
            {
                if (request.Content is null)
                    requestErrors.Add("$: required request body is missing");
                else
                {
                    var media = request.Content.Headers.ContentType?.MediaType;
                    if (media is null || !requestBody.GetProperty("content").TryGetProperty(media, out var representation))
                        requestErrors.Add("$: request media type is not declared");
                    else
                    {
                        try
                        {
                            using var body = JsonDocument.Parse(await request.Content.ReadAsByteArrayAsync(cancellationToken));
                            requestErrors.AddRange(JsonContractValidator.Check(representation.GetProperty("schema"), body.RootElement, components, request: true));
                        }
                        catch (JsonException) { requestErrors.Add("$: request is not valid JSON"); }
                    }
                }
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var responseErrors = new List<string>();
            if (status == 204)
            {
                if (bytes.Length != 0 || declared.TryGetProperty("content", out _))
                    responseErrors.Add("$: 204 contains or declares a body");
            }
            else
            {
                var media = response.Content.Headers.ContentType?.MediaType;
                var expected = status >= 400 ? "application/problem+json" : "application/json";
                if (media != expected || !declared.TryGetProperty("content", out var content) ||
                    !content.TryGetProperty(media, out var representation) || bytes.Length == 0)
                    responseErrors.Add("$: response media type/body differs from OpenAPI");
                else
                {
                    using var body = JsonDocument.Parse(bytes);
                    responseErrors.AddRange(JsonContractValidator.Check(representation.GetProperty("schema"), body.RootElement, components));
                }
            }
            record?.Invoke("OpenAPI exchange contract", new
            {
                exchangeId = request.Options.TryGetValue(HttpEvidenceHandler.ExchangeIdKey, out var exchange) ? exchange : null,
                stepPhase = request.Options.TryGetValue(HttpEvidenceHandler.StepPhaseKey, out var phase) ? phase : null,
                operationId = operation.GetProperty("operationId").GetString(),
                method = request.Method.Method,
                path,
                status,
                request = new { checkedParameters = parameterCount, checkedBody = operation.TryGetProperty("requestBody", out _), conforms = requestErrors.Count == 0, violations = requestErrors },
                response = new { conforms = responseErrors.Count == 0, violations = responseErrors },
                scope = "Generated schema shapes; business rules remain separate. Invalid request bodies are observed after HTTP, never blocked by this checker."
            });
            if (responseErrors.Count != 0 || status < 400 && requestErrors.Count != 0)
                throw new InvalidOperationException($"OpenAPI contract mismatch at {request.Method} {path} {status}: " +
                    string.Join("; ", responseErrors.Concat(status < 400 ? requestErrors : [])));
            return response;
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private async Task ValidateFrameworkProblem(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentType?.MediaType != "application/problem+json")
            throw new InvalidOperationException($"Framework error has unexpected media type: {(int)response.StatusCode}");
        using var body = JsonDocument.Parse(await response.Content.ReadAsByteArrayAsync(cancellationToken));
        var components = document.RootElement.GetProperty("components").GetProperty("schemas");
        var errors = JsonContractValidator.Check(components.GetProperty("ApiProblemDetails"), body.RootElement, components);
        if (errors.Count != 0 || body.RootElement.GetProperty("status").GetInt32() != (int)response.StatusCode)
            throw new InvalidOperationException($"Framework problem differs from generated schema: {string.Join("; ", errors)}");
        request.Options.TryGetValue(HttpEvidenceHandler.ExchangeIdKey, out var exchange);
        record?.Invoke("OpenAPI framework problem contract", new { exchangeId = exchange, status = (int)response.StatusCode, conforms = true });
    }

    private static bool Matches(string template, string path)
    {
        var expected = template.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var actual = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return expected.Length == actual.Length && expected.Zip(actual).All(parts =>
            parts.First.StartsWith('{') && parts.First.EndsWith('}') ||
            string.Equals(parts.First, parts.Second, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesPathParameters(string template, string path, JsonElement operation)
    {
        if (!operation.TryGetProperty("parameters", out var parameters))
            return true;
        var expected = template.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var actual = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var parameter in parameters.EnumerateArray())
        {
            if (parameter.GetProperty("in").GetString() != "path" ||
                !parameter.TryGetProperty("schema", out var schema) ||
                !schema.TryGetProperty("format", out var format) || format.GetString() != "uuid")
                continue;
            var segment = Array.IndexOf(expected, "{" + parameter.GetProperty("name").GetString() + "}");
            if (segment >= 0 && !Guid.TryParse(actual[segment], out _))
                return false;
        }
        return true;
    }

}

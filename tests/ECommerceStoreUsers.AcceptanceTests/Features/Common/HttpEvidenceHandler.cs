using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

/// <summary>Buffers actual HTTP content without replacing it or changing contract validation.</summary>
internal sealed class HttpEvidenceHandler(ScenarioHttpEvidence evidence) : DelegatingHandler
{
    internal static readonly HttpRequestOptionsKey<string> ExchangeIdKey = new("Users.HttpEvidence.ExchangeId");
    internal static readonly HttpRequestOptionsKey<string> StepPhaseKey = new("Users.HttpEvidence.StepPhase");

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sequence = evidence.NextExchange();
        var exchangeId = $"{evidence.InstanceId}/{sequence}";
        var phase = evidence.StepPhase;
        request.Options.Set(ExchangeIdKey, exchangeId);
        request.Options.Set(StepPhaseKey, phase);
        var method = request.Method.Method;
        var path = request.RequestUri?.AbsolutePath;
        var query = request.RequestUri?.Query;
        evidence.Record($"HTTP {sequence:000} request", new
        {
            service = "users",
            scenarioId = evidence.ScenarioId,
            instanceId = evidence.InstanceId,
            exchangeId,
            stepPhase = phase,
            method,
            path,
            query,
            content = await ReadContent(request.Content, cancellationToken)
        });

        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        catch (Exception error)
        {
            evidence.Record($"HTTP {sequence:000} failure", new
            {
                service = "users",
                scenarioId = evidence.ScenarioId,
                instanceId = evidence.InstanceId,
                exchangeId,
                stepPhase = phase,
                method,
                path,
                query,
                exceptionType = error.GetType().Name,
                cancelled = error is OperationCanceledException
            });
            throw;
        }

        // Record before an outer contract validator or the scenario assertions can fail.
        evidence.Record($"HTTP {sequence:000} response", new
        {
            service = "users",
            scenarioId = evidence.ScenarioId,
            instanceId = evidence.InstanceId,
            exchangeId,
            stepPhase = phase,
            method,
            path,
            query,
            status = (int)response.StatusCode,
            location = response.Headers.Location?.ToString(),
            content = await ReadContent(response.Content, cancellationToken)
        });
        return response;
    }

    private static async Task<object> ReadContent(HttpContent? content, CancellationToken cancellationToken)
    {
        if (content is null)
            return new { contentType = (string?)null, bodyFormat = "none", body = (object?)null, rawBody = (string?)null, byteLength = 0, bodyBytesBase64 = (string?)null };

        var bytes = await content.ReadAsByteArrayAsync(cancellationToken);
        var raw = await content.ReadAsStringAsync(cancellationToken);
        var mediaType = content.Headers.ContentType?.MediaType;
        object? body = raw;
        var format = bytes.Length == 0 ? "empty" : "text";
        if (bytes.Length == 0)
            body = null;
        else if (mediaType?.EndsWith("/json", StringComparison.OrdinalIgnoreCase) == true ||
                 mediaType?.EndsWith("+json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                using var document = JsonDocument.Parse(raw);
                body = document.RootElement.Clone();
                format = "json";
            }
            catch (JsonException)
            {
                format = "invalid-json";
            }
        }
        return new { contentType = content.Headers.ContentType?.ToString(), bodyFormat = format, body, rawBody = raw, byteLength = bytes.Length, bodyBytesBase64 = Convert.ToBase64String(bytes) };
    }
}


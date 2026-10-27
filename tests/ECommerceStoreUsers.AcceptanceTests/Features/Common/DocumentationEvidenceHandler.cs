using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

internal sealed class DocumentationEvidenceHandler(JsonDocument document, ScenarioHttpEvidence evidence, DocumentationEvidence documentation) : DelegatingHandler
{
    internal static readonly HttpRequestOptionsKey<TaskCompletionSource> CompletionKey = new("Users.DocumentationEvidence.Completion");

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response;
            try { response = await base.SendAsync(request, cancellationToken); }
            catch (Exception error)
            {
                documentation.ObserveFailure(evidence, request, OperationId(request), error.GetType().Name);
                throw;
            }
            try
            {
                await documentation.Observe(evidence, request, response, OperationId(request), cancellationToken);
                return response;
            }
            catch { response.Dispose(); throw; }
        }
        finally
        {
            if (request.Options.TryGetValue(CompletionKey, out var completion)) completion.TrySetResult();
        }
    }

    private string? OperationId(HttpRequestMessage request)
    {
        var route = OpenApiResponseHandler.FindRoute(document, request);
        return route.Value.ValueKind == JsonValueKind.Undefined ? null
            : route.Value.GetProperty(request.Method.Method.ToLowerInvariant()).GetProperty("operationId").GetString();
    }
}

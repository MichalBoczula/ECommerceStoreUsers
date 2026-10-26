using ECommerceStoreUsers.API;
using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common
{
    public sealed class ScenarioApiContext
    {
        private ScenarioHttpEvidence? _evidence;
        public string DatabaseName { get; set; } = string.Empty;
        public ApplicationFactory Factory { get; set; } = default!;
        public HttpClient HttpClient { get; set; } = default!;
        public JsonDocument OpenApiDocument { get; set; } = default!;
        public HttpResponseMessage? Response { get; set; }
        public Guid FavoriteClientId { get; set; }
        public Guid FavoriteProductId { get; set; }

        public void InitializeEvidence(ScenarioInfo scenario, string feature)
            => _evidence = ScenarioHttpEvidence.From(scenario, feature);

        public void BeginStep(string phase) => Evidence.StepPhase = phase;
        public void PublishEvidenceIdentity() => Evidence.PublishIdentity();
        public void FlushEvidence() => Evidence.Flush();

        public void RecordEvidence(string name, object value) => Evidence.Record(name, new
        {
            service = "users",
            scenarioId = Evidence.ScenarioId,
            instanceId = Evidence.InstanceId,
            value
        });

        public HttpClient CreateClient(WebApplicationFactory<Program> factory)
            // Capture inside validation so the real response survives a contract failure.
            => factory.CreateDefaultClient(new OpenApiResponseHandler(OpenApiDocument), new HttpEvidenceHandler(Evidence));

        private ScenarioHttpEvidence Evidence => _evidence ?? throw new InvalidOperationException("Scenario evidence is not initialized.");

        public JsonSerializerOptions JsonOptions { get; } = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }
}


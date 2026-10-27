using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Configuration;

public sealed class DocumentationEvidenceTests
{
    [Fact]
    public async Task SetupAndExplicitSupportingExchangeDoNotClaimThePrimaryOutcome()
    {
        var evidence = Evidence("REF07_CreateCustomer_04");
        var links = new DocumentationEvidence(evidence.ScenarioId);
        using var response = Response(200);
        using var request = Request(evidence, "Given");
        await links.Observe(evidence, request, response, "CreateCustomer", default);
        request.Options.Set(HttpEvidenceHandler.StepPhaseKey, "When");
        request.Options.Set(HttpEvidenceHandler.SupportingExchangeKey, true);
        await links.Observe(evidence, request, response, "CreateCustomer", default);
        request.Options.Set(HttpEvidenceHandler.SupportingExchangeKey, false);
        await links.Observe(evidence, request, response, "CreateCustomer", default);
        links.Complete();
        var rows = Drain(evidence);
        Assert.Equal(new[] { "supporting-or-technical-exchange", "supporting-or-technical-exchange", "primary-scenario-outcome" }, rows.Select(row => row.GetProperty("role").GetString()));
        Assert.Equal("users.branch.create-customer.saved", rows[2].GetProperty("expectedBranchId").GetString());
    }

    [Fact]
    public async Task DuplicatePrimaryExchangeFailsCompletion()
    {
        var evidence = Evidence("REF07_CreateCustomer_04");
        var links = new DocumentationEvidence(evidence.ScenarioId);
        using var request = Request(evidence, "When");
        using var response = Response(200);
        await links.Observe(evidence, request, response, "CreateCustomer", default);
        await links.Observe(evidence, request, response, "CreateCustomer", default);
        Assert.Throws<InvalidOperationException>(links.Complete);
    }

    [Fact]
    public async Task ExpectedRuleMustOccurInTheActualRejection()
    {
        var evidence = Evidence("REF06_Validation_CreateCustomer_ExternalBlank");
        var links = new DocumentationEvidence(evidence.ScenarioId);
        using var request = Request(evidence, "When");
        using var response = Response(400, "validation_failed");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => links.Observe(evidence, request, response, "CreateCustomer", default));
        Assert.Contains("Expected rule is absent", error.Message);
    }

    [Fact]
    public async Task UninstalledRuleCannotBePresentedAsEnforcedByTheFlow()
    {
        var evidence = Evidence("REF07_CreateCustomer_04");
        var links = new DocumentationEvidence(evidence.ScenarioId);
        using var request = Request(evidence, "Given");
        using var response = Response(400, "validation_failed", "CustomerCompanyTaxIdValidationRule");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => links.Observe(evidence, request, response, "CreateCustomer", default));
        Assert.Contains("uninstalled or unrelated rule", error.Message);
    }

    [Fact]
    public async Task MatchingStatusCannotSubstituteForTheDeclaredProblemCode()
    {
        var evidence = Evidence("REF06_Validation_CreateCustomer_ExternalBlank");
        var links = new DocumentationEvidence(evidence.ScenarioId);
        using var request = Request(evidence, "When");
        using var response = Response(400, "invalid_json");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => links.Observe(evidence, request, response, "CreateCustomer", default));
        Assert.Contains("status/problem code differs", error.Message);
    }

    [Fact]
    public void CancellationLinksNeverInventAResponseStatusOrObservedBranch()
    {
        var evidence = Evidence("REF09_Cancel_CreateCustomer");
        var links = new DocumentationEvidence(evidence.ScenarioId);
        using var request = Request(evidence, "When");
        links.ObserveFailure(evidence, request, "CreateCustomer", "HttpRequestException");
        links.Complete();
        var row = Assert.Single(Drain(evidence));
        Assert.Equal("transport-failure-without-response", row.GetProperty("role").GetString());
        Assert.Equal("users.flow.create-customer", row.GetProperty("nominalFlowId").GetString());
        Assert.False(row.TryGetProperty("status", out _));
        Assert.False(row.TryGetProperty("expectedBranchId", out _));
    }

    private static ScenarioHttpEvidence Evidence(string id) => new(id, "Documentation evidence", "Join actual exchange", new SortedDictionary<string, string?>());

    private static HttpRequestMessage Request(ScenarioHttpEvidence evidence, string phase)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/customers");
        request.Options.Set(HttpEvidenceHandler.ExchangeIdKey, evidence.InstanceId + "/1");
        request.Options.Set(HttpEvidenceHandler.StepPhaseKey, phase);
        return request;
    }

    private static HttpResponseMessage Response(int status, string? code = null, string? rule = null)
        => new((HttpStatusCode)status)
        {
            Content = new StringContent(code is null ? "{}" : JsonSerializer.Serialize(new
            {
                code,
                errors = rule is null ? Array.Empty<object>() : [new { name = rule, message = "Synthetic error", entity = "Customer" }]
            }), Encoding.UTF8, code is null ? "application/json" : "application/problem+json")
        };

    private static List<JsonElement> Drain(ScenarioHttpEvidence evidence)
    {
        var result = new List<JsonElement>();
        evidence.Flush((_, value) => result.Add(JsonSerializer.SerializeToElement(value, ScenarioHttpEvidence.JsonOptions)));
        return result;
    }
}

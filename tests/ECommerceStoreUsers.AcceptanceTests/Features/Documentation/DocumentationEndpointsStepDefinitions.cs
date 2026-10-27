using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.ResponsesDto;
using Reqnroll;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Documentation
{
    [Binding]
    public sealed class DocumentationEndpointsStepDefinitions
    {
        private static readonly string[] ExpectedFlowNames =
        [
            "GetCreateCustomerDescriptor",
            "GetCustomerByExternalIdDescriptor",
            "GetUpdateIndividualDataDescriptor",
            "GetAddCompanyDescriptor",
            "GetUpdateCompanyDescriptor",
            "GetGetAdminByExternalIdDescriptor",
            "GetCreateAdminDescriptor",
            "GetUpdateAdminProfileDescriptor",
            "GetGetFavoritesByClientIdDescriptor",
            "GetAddProductToFavoritesDescriptor",
            "GetRemoveProductFromFavoritesDescriptor",
            "GetClearClientFavoritesDescriptor"
        ];

        private static readonly string[] ExpectedPolicyNames =
        [
            "CustomerValidationPolicy",
            "IndividualDataValidationPolicy",
            "CompanyValidationPolicy",
            "AdminValidationPolicy",
            "EmptyGuidValidationPolicy",
            "FavoriteValidationPolicy"
        ];

        private readonly ScenarioApiContext _apiContext;

        public DocumentationEndpointsStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [When("I request the flow documentation")]
        public async Task WhenIRequestTheFlowDocumentation()
        {
            _apiContext.Response = await _apiContext.HttpClient.GetAsync(
                "/users-documentation/flows");
        }

        [When("I request the validation documentation")]
        public async Task WhenIRequestTheValidationDocumentation()
        {
            _apiContext.Response = await _apiContext.HttpClient.GetAsync(
                "/users-documentation/validations");
        }

        [Then("all 12 flow descriptors are returned with status 200")]
        public async Task ThenAllFlowDescriptorsAreReturnedWithStatus200()
        {
            RequireSuccessfulJsonResponse();

            var response = await Deserialize<FlowDescriptorsResponseDto>();
            response.ShouldNotBeNull();
            response.Flows.Count.ShouldBe(1);

            var flows = response.Flows.Single();
            flows.Keys.ShouldBe(ExpectedFlowNames, ignoreOrder: true);
            flows.Values.ShouldAllBe(flow =>
                !string.IsNullOrWhiteSpace(flow.FlowName) &&
                flow.Steps.Count > 0);
        }

        [Then("all 6 validation policies are returned with status 200")]
        public async Task ThenAllValidationPoliciesAreReturnedWithStatus200()
        {
            RequireSuccessfulJsonResponse();

            var response = await Deserialize<ValidationDescriptorsResponseDto>();
            response.ShouldNotBeNull();
            response.Validations.Count.ShouldBe(ExpectedPolicyNames.Length);

            var policies = response.Validations
                .SelectMany(validation => validation)
                .ToDictionary(entry => entry.Key, entry => entry.Value);

            policies.Keys.ShouldBe(ExpectedPolicyNames, ignoreOrder: true);
            policies.ShouldAllBe(entry =>
                entry.Key == entry.Value.PolicyName &&
                entry.Value.Rules.Count > 0);
        }

        [Then("flow documentation has these exact descriptors")]
        public async Task ThenExactFlows(Table table)
        {
            var response = await Deserialize<FlowDescriptorsResponseDto>();
            response.ShouldNotBeNull();
            var flows = response.Flows.ShouldHaveSingleItem();
            flows.Keys.Order().ShouldBe(table.Rows.Select(row => row["Descriptor"]).Order());
            foreach (var row in table.Rows)
            {
                var flow = flows[row["Descriptor"]];
                flow.FlowName.ShouldBe(row["Flow"]);
                var count = int.Parse(row["Steps"], System.Globalization.CultureInfo.InvariantCulture);
                flow.Steps.Count.ShouldBe(count);
                flow.Steps.Select(step => step.Order).ShouldBe(Enumerable.Range(1, count));
                flow.Steps.Select(step => step.BpmnId).Distinct().Count().ShouldBe(count);
                foreach (var step in flow.Steps)
                {
                    step.StepName.ShouldNotBeNullOrWhiteSpace();
                    step.BpmnId.ShouldNotBeNullOrWhiteSpace();
                }
            }
            flows.Values.Sum(flow => flow.Steps.Count).ShouldBe(92);
            _apiContext.RecordEvidence("Technical flow expectations", table.Rows.Select(row => new { descriptor = row["Descriptor"], flow = row["Flow"], steps = row["Steps"] }).ToArray());
            _apiContext.RecordEvidence("Technical flow metadata verified", new { descriptors = flows.Count, steps = 92, orderedSteps = true, completeStepMetadata = true });
        }

        [Then("validation documentation has these exact policies")]
        public async Task ThenExactPolicies(Table table)
        {
            var response = await Deserialize<ValidationDescriptorsResponseDto>();
            response.ShouldNotBeNull();
            var policies = response.Validations.SelectMany(item => item).ToDictionary(item => item.Key, item => item.Value);
            policies.Keys.Order().ShouldBe(table.Rows.Select(row => row["Policy"]).Order());
            foreach (var row in table.Rows)
            {
                var policy = policies[row["Policy"]];
                policy.PolicyName.ShouldBe(row["Policy"]);
                policy.Rules.Count.ShouldBe(int.Parse(row["Rules"], System.Globalization.CultureInfo.InvariantCulture));
                policy.Rules.Sum(rule => rule.Rules.Count).ShouldBe(int.Parse(row["Errors"], System.Globalization.CultureInfo.InvariantCulture));
                policy.Rules.Select(rule => rule.RuleName).Distinct().Count().ShouldBe(policy.Rules.Count);
                foreach (var rule in policy.Rules)
                {
                    rule.RuleName.ShouldNotBeNullOrWhiteSpace();
                    rule.Rules.ShouldNotBeEmpty();
                    foreach (var error in rule.Rules)
                    {
                        error.Name.ShouldBe(rule.RuleName);
                        error.Entity.ShouldNotBeNullOrWhiteSpace();
                        error.Message.ShouldNotBeNullOrWhiteSpace();
                    }
                }
            }
            var errors = policies.Values.SelectMany(policy => policy.Rules).SelectMany(rule => rule.Rules).ToArray();
            errors.Select(error => (error.Name, error.Message, error.Entity)).Distinct().Count().ShouldBe(20);
            _apiContext.RecordEvidence("Technical policy expectations", table.Rows.Select(row => new { policy = row["Policy"], rules = row["Rules"], errors = row["Errors"] }).ToArray());
            _apiContext.RecordEvidence("Technical policy metadata verified", new { policies = policies.Count, ruleEntries = 35, errorEntries = errors.Length, distinctSignatures = 20, completeErrorTriplets = true });
        }

        [When("I request the generated OpenAPI document")]
        public async Task WhenOpenApi() => _apiContext.Response = await _apiContext.HttpClient.GetAsync("/swagger/v1/swagger.json");

        [Then("the generated OpenAPI has these values")]
        public async Task ThenOpenApi(Table table)
        {
            RequireSuccessfulJsonResponse();
            using var document = JsonDocument.Parse(await _apiContext.Response!.Content.ReadAsStringAsync());
            JsonElement.DeepEquals(document.RootElement, _apiContext.OpenApiDocument.RootElement).ShouldBeTrue();
            document.RootElement.GetProperty("openapi").GetString()!.StartsWith("3.", StringComparison.Ordinal).ShouldBeTrue();
            var operations = document.RootElement.GetProperty("paths").EnumerateObject().SelectMany(path => path.Value.EnumerateObject())
                .Where(method => method.Value.ValueKind == JsonValueKind.Object && method.Value.TryGetProperty("operationId", out _))
                .Select(method => method.Value.GetProperty("operationId").GetString()).ToArray();
            var expected = table.Rows.ToDictionary(row => row["Field"], row => int.Parse(row["Value"], System.Globalization.CultureInfo.InvariantCulture));
            operations.Length.ShouldBe(expected["NamedOperations"]);
            operations.Distinct().Count().ShouldBe(operations.Length);
            document.RootElement.GetProperty("components").GetProperty("schemas").EnumerateObject().Count().ShouldBe(expected["Schemas"]);
            _apiContext.RecordEvidence("Technical OpenAPI verified", new { expectations = expected, operations, bootstrapDocumentEqual = true, fullPropertyValidation = "Deferred to task 10" });
        }

        private void RequireSuccessfulJsonResponse()
        {
            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            _apiContext.Response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        }

        private async Task<T?> Deserialize<T>()
        {
            _apiContext.Response.ShouldNotBeNull();

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson(
                $"Documentation response ({(int)_apiContext.Response.StatusCode})",
                body);

            return JsonSerializer.Deserialize<T>(body, _apiContext.JsonOptions);
        }
    }
}


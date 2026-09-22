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

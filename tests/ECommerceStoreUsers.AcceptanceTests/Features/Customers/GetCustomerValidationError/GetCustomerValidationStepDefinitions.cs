using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.GetCustomerValidationError
{
    [Binding]
    public class GetCustomerValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private string? _externalId;

        public GetCustomerValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have an invalid get customer request")]
        public void GivenIHaveAnInvalidGetCustomerRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Get customer invalid request table", requestValues, _apiContext.JsonOptions);

            _externalId = GetRequiredValue(requestValues, "ExternalId");

            AllureJson.AttachObject(
                "Get customer invalid request contract",
                new { ExternalId = _externalId },
                _apiContext.JsonOptions);
        }

        [When("I submit the get customer request for validation")]
        public async Task WhenISubmitTheGetCustomerRequestForValidation()
        {
            _externalId.ShouldNotBeNull();

            var encodedExternalId = Uri.EscapeDataString(_externalId!);
            _apiContext.Response = await _apiContext.HttpClient.GetAsync($"/customers/external/{encodedExternalId}");

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Get Customer Validation Response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the get customer request fails with a validation error")]
        public async Task ThenTheGetCustomerRequestFailsWithAValidationError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected get customer validation response table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<ApiProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();
            problemDetails.Errors.ShouldNotBeEmpty();

            var expectedMessage = GetRequiredValue(expected, "Message");
            var expectedName = GetRequiredValue(expected, "Name");
            var expectedEntity = GetRequiredValue(expected, "Entity");

            var externalIdError = problemDetails.Errors.FirstOrDefault(e => e.Name == expectedName);

            externalIdError.ShouldNotBeNull($"Expected validation rule '{expectedName}' was not triggered.");
            externalIdError.Message.ShouldBe(expectedMessage);
            externalIdError.Entity.ShouldBe(expectedEntity);

            problemDetails.TraceId.ShouldNotBeNullOrWhiteSpace();
        }

        private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _apiContext.JsonOptions);
        }

        private static Dictionary<string, string> ParseExpectedTable(Table table)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in table.Rows)
            {
                values[row["Field"]] = row["Value"];
            }

            return values;
        }

        private static string GetRequiredValue(IReadOnlyDictionary<string, string> values, string key)
        {
            if (!values.TryGetValue(key, out var value))
            {
                throw new InvalidOperationException($"Missing '{key}' validation key value in data table.");
            }

            return value;
        }

        private static HttpStatusCode ParseStatusCode(IReadOnlyDictionary<string, string> values, string key)
        {
            var value = GetRequiredValue(values, key);
            return (HttpStatusCode)int.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}

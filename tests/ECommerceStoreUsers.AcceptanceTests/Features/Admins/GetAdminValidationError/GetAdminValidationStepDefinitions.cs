using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins.GetAdminValidationError
{
    [Binding]
    public class GetAdminValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private string? _externalId;

        public GetAdminValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have an invalid get admin external id")]
        public void GivenIHaveAnInvalidGetAdminExternalId(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Get admin invalid external id table", requestValues, _apiContext.JsonOptions);

            _externalId = ResolveExternalId(GetRequiredValue(requestValues, "ExternalId"));

            AllureJson.AttachObject(
                "Get admin invalid external id value",
                new { ExternalId = _externalId },
                _apiContext.JsonOptions);
        }

        [When("I submit the get admin request for validation")]
        public async Task WhenISubmitTheGetAdminRequestForValidation()
        {
            _externalId.ShouldNotBeNull();

            var encodedExternalId = Uri.EscapeDataString(_externalId!);
            _apiContext.Response = await _apiContext.HttpClient.GetAsync($"/admins/{encodedExternalId}");

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Get Admin Validation Response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the get admin request fails with a validation error")]
        public async Task ThenTheGetAdminRequestFailsWithAValidationError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected get admin validation result table", expected, _apiContext.JsonOptions);

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

        private static string ResolveExternalId(string value)
        {
            return value.Equals("<whitespace>", StringComparison.OrdinalIgnoreCase)
                ? "   "
                : value;
        }

        private static HttpStatusCode ParseStatusCode(IReadOnlyDictionary<string, string> values, string key)
        {
            var value = GetRequiredValue(values, key);
            return (HttpStatusCode)int.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}

using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests
{
    [Binding]
    public class CreateAdminProfileDeserializationValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private Dictionary<string, string>? _rawPayload;

        public CreateAdminProfileDeserializationValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have a create admin request payload missing the external id")]
        public void GivenIHaveACreateAdminRequestPayloadMissingTheExternalId(Table table)
        {
            _rawPayload = ParseExpectedTable(table);

            AllureJson.AttachObject("Incomplete raw request JSON dictionary structure", _rawPayload, _apiContext.JsonOptions);
        }

        [When("I submit the incomplete create admin request")]
        public async Task WhenISubmitTheIncompleteCreateAdminRequest()
        {
            _rawPayload.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/admins",
                _rawPayload,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Deserialization Exception Response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the response indicates a json deserialization failure")]
        public void ThenTheResponseIndicatesAJsonDeserializationFailure(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected deserialization error constraints", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();

            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));
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
                throw new InvalidOperationException($"Missing '{key}' mapping key constraint inside data table.");
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
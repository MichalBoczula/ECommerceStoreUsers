using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.GetCustomerNotFound
{
    [Binding]
    public class GetCustomerNotFoundStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private string? _externalId;

        public GetCustomerNotFoundStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have a missing customer external id")]
        public void GivenIHaveAMissingCustomerExternalId(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Missing customer lookup table", requestValues, _apiContext.JsonOptions);

            _externalId = GetRequiredValue(requestValues, "ExternalId");
        }

        [When("I request the customer profile by external id")]
        public async Task WhenIRequestTheCustomerProfileByExternalId()
        {
            _externalId.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.GetAsync(
                $"/customers/external/{_externalId}");

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Not Found Response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the customer profile request fails with a not found response")]
        public async Task ThenTheCustomerProfileRequestFailsWithANotFoundResponse(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected customer not found result table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<NotFoundProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();
            problemDetails!.Status.ShouldBe((int)ParseStatusCode(expected, "StatusCode"));
            problemDetails.Title.ShouldBe(GetRequiredValue(expected, "Title"));
            problemDetails.Detail.ShouldBe(GetRequiredValue(expected, "Detail"));
            problemDetails.Type.ShouldBe(GetRequiredValue(expected, "Type"));
            problemDetails.Instance.ShouldBe(GetRequiredValue(expected, "Instance"));

            if (TryGetBool(expected, "HasTraceId", out var hasTraceId))
            {
                if (hasTraceId)
                {
                    problemDetails.TraceId.ShouldNotBeNullOrWhiteSpace();
                }
                else
                {
                    problemDetails.TraceId.ShouldBeNullOrWhiteSpace();
                }
            }
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
                throw new InvalidOperationException($"Missing '{key}' not found key value in data table.");
            }

            return value;
        }

        private static HttpStatusCode ParseStatusCode(IReadOnlyDictionary<string, string> values, string key)
        {
            var value = GetRequiredValue(values, key);
            return (HttpStatusCode)int.Parse(value, CultureInfo.InvariantCulture);
        }

        private static bool TryGetBool(IReadOnlyDictionary<string, string> values, string key, out bool result)
        {
            if (!values.TryGetValue(key, out var value))
            {
                result = false;
                return false;
            }

            result = bool.Parse(value);
            return true;
        }
    }
}

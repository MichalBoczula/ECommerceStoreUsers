using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.AddCompanyNotFound
{
    [Binding]
    public class AddCompanyNotFoundStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private Guid? _customerId;
        private AddCompanyRequestDto? _request;

        public AddCompanyNotFoundStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have an add company request for a missing customer")]
        public void GivenIHaveAnAddCompanyRequestForAMissingCustomer(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Missing customer add company request table", requestValues, _apiContext.JsonOptions);

            _customerId = Guid.Parse(GetRequiredValue(requestValues, "CustomerId"));
            _request = new AddCompanyRequestDto
            {
                TaxId = GetRequiredValue(requestValues, "TaxId"),
                CompanyName = GetRequiredValue(requestValues, "CompanyName"),
                BillingAddress = BuildAddress(requestValues, "Billing"),
                ShippingAddress = BuildAddress(requestValues, "Shipping")
            };

            AllureJson.AttachObject(
                "Missing customer add company request",
                new { CustomerId = _customerId, Request = _request },
                _apiContext.JsonOptions);
        }

        [When("I submit the add company request for the missing customer")]
        public async Task WhenISubmitTheAddCompanyRequestForTheMissingCustomer()
        {
            _customerId.ShouldNotBeNull();
            _request.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                $"/customers/{_customerId}/companies",
                _request,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Add Company Not Found Response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the add company request fails with a not found response")]
        public async Task ThenTheAddCompanyRequestFailsWithANotFoundResponse(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected add company not found response table", expected, _apiContext.JsonOptions);

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

        private static AddressRequestDto BuildAddress(IReadOnlyDictionary<string, string> values, string prefix)
        {
            return new AddressRequestDto
            {
                PostalCode = GetRequiredValue(values, $"{prefix}PostalCode"),
                City = GetRequiredValue(values, $"{prefix}City"),
                Street = GetRequiredValue(values, $"{prefix}Street"),
                BuildingNumber = GetRequiredValue(values, $"{prefix}BuildingNumber"),
                ApartmentNumber = GetRequiredValue(values, $"{prefix}ApartmentNumber")
            };
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
                throw new InvalidOperationException($"Missing '{key}' add company not found key value in data table.");
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

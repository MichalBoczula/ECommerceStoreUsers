using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.AddCompanyValidationError
{
    [Binding]
    public class AddCompanyValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private Guid? _customerId;
        private AddCompanyRequestDto? _request;

        public AddCompanyValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have an invalid add company request")]
        public void GivenIHaveAnInvalidAddCompanyRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Add company invalid request table", requestValues, _apiContext.JsonOptions);

            _customerId = Guid.Parse(GetRequiredValue(requestValues, "CustomerId"));
            _request = new AddCompanyRequestDto
            {
                TaxId = GetRequiredValue(requestValues, "TaxId"),
                CompanyName = GetRequiredValue(requestValues, "CompanyName"),
                BillingAddress = BuildAddress(requestValues, "Billing"),
                ShippingAddress = BuildAddress(requestValues, "Shipping")
            };

            AllureJson.AttachObject(
                "Add company invalid request",
                new { CustomerId = _customerId, Request = _request },
                _apiContext.JsonOptions);
        }

        [When("I submit the add company request for validation")]
        public async Task WhenISubmitTheAddCompanyRequestForValidation()
        {
            _customerId.ShouldNotBeNull();
            _request.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                $"/customers/{_customerId}/companies",
                _request,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Add Company Validation Response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the add company request fails with a validation error")]
        public async Task ThenTheAddCompanyRequestFailsWithAValidationError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected add company validation response table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<ApiProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();
            problemDetails.Errors.ShouldNotBeEmpty();

            var expectedMessage = GetRequiredValue(expected, "Message");
            var expectedName = GetRequiredValue(expected, "Name");
            var expectedEntity = GetRequiredValue(expected, "Entity");

            var validationError = problemDetails.Errors.FirstOrDefault(e => e.Name == expectedName);

            validationError.ShouldNotBeNull($"Expected validation rule '{expectedName}' was not triggered.");
            validationError.Message.ShouldBe(expectedMessage);
            validationError.Entity.ShouldBe(expectedEntity);

            problemDetails.TraceId.ShouldNotBeNullOrWhiteSpace();
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
                throw new InvalidOperationException($"Missing '{key}' add company validation key value in data table.");
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

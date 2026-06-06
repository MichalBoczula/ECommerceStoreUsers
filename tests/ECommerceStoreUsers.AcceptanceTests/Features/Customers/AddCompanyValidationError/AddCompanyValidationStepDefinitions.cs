using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
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
        private CreateCustomerRequestDto? _createCustomerRequest;
        private AddCompanyRequestDto? _addCompanyRequest;
        private Guid _customerId;

        public AddCompanyValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer exists for add company validation request")]
        public async Task GivenACustomerExistsForAddCompanyValidationRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Add company validation setup customer request table", requestValues, _apiContext.JsonOptions);

            _createCustomerRequest = new CreateCustomerRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExternalId"),
                Individual = new IndividualDataRequestDto
                {
                    FirstName = GetRequiredValue(requestValues, "FirstName"),
                    LastName = GetRequiredValue(requestValues, "LastName"),
                    Email = GetRequiredValue(requestValues, "Email"),
                    Phone = GetRequiredValue(requestValues, "Phone"),
                    BillingAddress = BuildAddress(requestValues, "Billing"),
                    ShippingAddress = BuildAddress(requestValues, "Shipping")
                }
            };

            AllureJson.AttachObject(
                "Create customer setup request contract for add company validation",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var body = await response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create customer setup response contract for add company validation ({(int)response.StatusCode})", body);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var customerResponse = await DeserializeResponse<CustomerResponseDto>(response);
            customerResponse.ShouldNotBeNull();
            customerResponse!.Id.ShouldNotBe(Guid.Empty);

            _customerId = customerResponse.Id;
        }

        [Given("I have an invalid add company request")]
        public void GivenIHaveAnInvalidAddCompanyRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Invalid add company request table", requestValues, _apiContext.JsonOptions);

            _addCompanyRequest = new AddCompanyRequestDto
            {
                TaxId = GetRequiredValue(requestValues, "TaxId"),
                CompanyName = GetRequiredValue(requestValues, "CompanyName"),
                BillingAddress = BuildAddress(requestValues, "Billing"),
                ShippingAddress = BuildAddress(requestValues, "Shipping")
            };

            AllureJson.AttachObject(
                "Invalid add company request contract",
                _addCompanyRequest,
                _apiContext.JsonOptions);
        }

        [When("I submit the add company request for validation")]
        public async Task WhenISubmitTheAddCompanyRequestForValidation()
        {
            _customerId.ShouldNotBe(Guid.Empty);
            _addCompanyRequest.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                $"/customers/{_customerId}/companies",
                _addCompanyRequest,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Add company validation response contract ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("adding the company fails with a validation error")]
        public async Task ThenAddingTheCompanyFailsWithAValidationError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected add company validation result table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<ApiProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();
            problemDetails.Errors.ShouldNotBeEmpty();

            var expectedMessage = GetRequiredValue(expected, "Message");
            var expectedName = GetRequiredValue(expected, "Name");
            var expectedEntity = GetRequiredValue(expected, "Entity");

            var companyDataError = problemDetails.Errors.FirstOrDefault(e => e.Name == expectedName);

            companyDataError.ShouldNotBeNull($"Expected validation rule '{expectedName}' was not triggered.");
            companyDataError.Message.ShouldBe(expectedMessage);
            companyDataError.Entity.ShouldBe(expectedEntity);

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

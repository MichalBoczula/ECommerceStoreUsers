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

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.UpdateCompanyValidationError
{
    [Binding]
    public class UpdateCompanyValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateCustomerRequestDto? _createCustomerRequest;
        private UpdateCompanyRequestDto? _updateCompanyRequest;
        private Guid _customerId;
        private Guid _companyId;

        public UpdateCompanyValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer with company exists for update company validation request")]
        public async Task GivenACustomerWithCompanyExistsForUpdateCompanyValidationRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update company validation setup customer with company request table", requestValues, _apiContext.JsonOptions);

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
                },
                Companies =
                [
                    new AddCompanyRequestDto
                    {
                        TaxId = GetRequiredValue(requestValues, "CompanyTaxId"),
                        CompanyName = GetRequiredValue(requestValues, "CompanyName"),
                        BillingAddress = BuildAddress(requestValues, "CompanyBilling"),
                        ShippingAddress = BuildAddress(requestValues, "CompanyShipping")
                    }
                ]
            };

            AllureJson.AttachObject(
                "Create customer setup request contract for update company validation",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var body = await response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create customer setup response contract for update company validation ({(int)response.StatusCode})", body);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var customerResponse = await DeserializeResponse<CustomerResponseDto>(response);
            customerResponse.ShouldNotBeNull();
            customerResponse!.Id.ShouldNotBe(Guid.Empty);

            var company = customerResponse.Companies.ShouldHaveSingleItem();
            company.Id.ShouldNotBe(Guid.Empty);

            _customerId = customerResponse.Id;
            _companyId = company.Id;
        }

        [Given("I have an invalid update company request")]
        public void GivenIHaveAnInvalidUpdateCompanyRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Invalid update company request table", requestValues, _apiContext.JsonOptions);

            _updateCompanyRequest = new UpdateCompanyRequestDto
            {
                TaxId = GetRequiredValue(requestValues, "TaxId"),
                CompanyName = GetRequiredValue(requestValues, "CompanyName"),
                BillingAddress = BuildAddress(requestValues, "Billing"),
                ShippingAddress = BuildAddress(requestValues, "Shipping")
            };

            AllureJson.AttachObject(
                "Invalid update company request contract",
                _updateCompanyRequest,
                _apiContext.JsonOptions);
        }

        [When("I submit the update company request for validation")]
        public async Task WhenISubmitTheUpdateCompanyRequestForValidation()
        {
            _customerId.ShouldNotBe(Guid.Empty);
            _companyId.ShouldNotBe(Guid.Empty);
            _updateCompanyRequest.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PutAsJsonAsync(
                $"/customers/{_customerId}/companies/{_companyId}",
                _updateCompanyRequest,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Update company validation response contract ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("updating the company fails with a validation error")]
        public async Task ThenUpdatingTheCompanyFailsWithAValidationError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected update company validation result table", expected, _apiContext.JsonOptions);

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

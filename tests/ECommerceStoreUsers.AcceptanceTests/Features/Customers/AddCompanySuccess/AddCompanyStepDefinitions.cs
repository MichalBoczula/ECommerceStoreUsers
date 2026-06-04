using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.AddCompanySuccess
{
    [Binding]
    public class AddCompanyStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateCustomerRequestDto? _createCustomerRequest;
        private AddCompanyRequestDto? _addCompanyRequest;
        private Guid _customerId;

        public AddCompanyStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer exists for add company request")]
        public async Task GivenACustomerExistsForAddCompanyRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Add company setup customer request table", requestValues, _apiContext.JsonOptions);

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
                "Create customer setup request contract",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var body = await response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create customer setup response contract ({(int)response.StatusCode})", body);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var customerResponse = await DeserializeResponse<CustomerResponseDto>(response);
            customerResponse.ShouldNotBeNull();
            customerResponse!.Id.ShouldNotBe(Guid.Empty);

            _customerId = customerResponse.Id;
        }

        [Given("I have a valid add company request")]
        public void GivenIHaveAValidAddCompanyRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Add company request table", requestValues, _apiContext.JsonOptions);

            _addCompanyRequest = new AddCompanyRequestDto
            {
                TaxId = GetRequiredValue(requestValues, "TaxId"),
                CompanyName = GetRequiredValue(requestValues, "CompanyName"),
                BillingAddress = BuildAddress(requestValues, "Billing"),
                ShippingAddress = BuildAddress(requestValues, "Shipping")
            };

            AllureJson.AttachObject(
                "Add company request contract",
                _addCompanyRequest,
                _apiContext.JsonOptions);
        }

        [When("I submit the add company request")]
        public async Task WhenISubmitTheAddCompanyRequest()
        {
            _customerId.ShouldNotBe(Guid.Empty);
            _addCompanyRequest.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                $"/customers/{_customerId}/companies",
                _addCompanyRequest,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Add company response contract ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the company is added to the customer successfully")]
        public async Task ThenTheCompanyIsAddedToTheCustomerSuccessfully(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected add company result table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var customerResponse = await DeserializeResponse<CustomerResponseDto>(_apiContext.Response);
            customerResponse.ShouldNotBeNull();

            if (TryGetBool(expected, "HasCustomerId", out var hasCustomerId))
            {
                if (hasCustomerId)
                {
                    customerResponse!.Id.ShouldNotBe(Guid.Empty);
                    customerResponse.Id.ShouldBe(_customerId);
                }
                else
                {
                    customerResponse!.Id.ShouldBe(Guid.Empty);
                }
            }

            customerResponse!.ExternalId.ShouldBe(GetExpectedValue(expected, "ExternalId", customerResponse.ExternalId));
            customerResponse.Companies.Count.ShouldBe(ParseInt(expected, "CompanyCount"));

            var company = customerResponse.Companies.ShouldHaveSingleItem();

            if (TryGetBool(expected, "HasCompanyId", out var hasCompanyId))
            {
                if (hasCompanyId)
                {
                    company.Id.ShouldNotBe(Guid.Empty);
                }
                else
                {
                    company.Id.ShouldBe(Guid.Empty);
                }
            }

            company.TaxId.ShouldBe(GetExpectedValue(expected, "TaxId", company.TaxId));
            company.CompanyName.ShouldBe(GetExpectedValue(expected, "CompanyName", company.CompanyName));
            AssertAddress(company.BillingAddress, expected, "Billing");
            AssertAddress(company.ShippingAddress, expected, "Shipping");

            AllureJson.AttachObject(
                "Verified add company response contract",
                customerResponse,
                _apiContext.JsonOptions);
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

        private static void AssertAddress(AddressResponseDto actual, IReadOnlyDictionary<string, string> expected, string prefix)
        {
            actual.PostalCode.ShouldBe(GetExpectedValue(expected, $"{prefix}PostalCode", actual.PostalCode));
            actual.City.ShouldBe(GetExpectedValue(expected, $"{prefix}City", actual.City));
            actual.Street.ShouldBe(GetExpectedValue(expected, $"{prefix}Street", actual.Street));
            actual.BuildingNumber.ShouldBe(GetExpectedValue(expected, $"{prefix}BuildingNumber", actual.BuildingNumber));
            actual.ApartmentNumber.ShouldBe(GetExpectedValue(expected, $"{prefix}ApartmentNumber", actual.ApartmentNumber!));
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
                throw new InvalidOperationException($"Missing '{key}' value in add company data table.");
            }

            return value;
        }

        private static string GetExpectedValue(IReadOnlyDictionary<string, string> values, string key, string fallback)
        {
            return values.TryGetValue(key, out var value) ? value : fallback;
        }

        private static HttpStatusCode ParseStatusCode(IReadOnlyDictionary<string, string> values, string key)
        {
            var value = GetRequiredValue(values, key);
            return (HttpStatusCode)int.Parse(value, CultureInfo.InvariantCulture);
        }

        private static int ParseInt(IReadOnlyDictionary<string, string> values, string key)
        {
            var value = GetRequiredValue(values, key);
            return int.Parse(value, CultureInfo.InvariantCulture);
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

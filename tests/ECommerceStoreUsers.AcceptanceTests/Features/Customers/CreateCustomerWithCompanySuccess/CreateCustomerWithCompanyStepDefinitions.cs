using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.CreateCustomerWithCompanySuccess
{
    [Binding]
    public class CreateCustomerWithCompanyStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateCustomerRequestDto? _createRequest;
        private AddCompanyRequestDto? _addCompanyRequest;
        private Guid _customerId;

        public CreateCustomerWithCompanyStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have a valid create customer request")]
        public void GivenIHaveAValidCreateCustomerRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Create customer request table", requestValues, _apiContext.JsonOptions);

            _createRequest = new CreateCustomerRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExternalId"),
                Individual = new IndividualDataRequestDto
                {
                    FirstName = GetRequiredValue(requestValues, "Individual.FirstName"),
                    LastName = GetRequiredValue(requestValues, "Individual.LastName"),
                    Email = GetRequiredValue(requestValues, "Individual.Email"),
                    Phone = GetRequiredValue(requestValues, "Individual.Phone"),
                    BillingAddress = CreateAddress(requestValues, "Individual.BillingAddress"),
                    ShippingAddress = CreateAddress(requestValues, "Individual.ShippingAddress")
                }
            };

            AllureJson.AttachObject("Create customer request", _createRequest, _apiContext.JsonOptions);
        }

        [When("I submit the create customer request")]
        public async Task WhenISubmitTheCreateCustomerRequest()
        {
            _createRequest.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _createRequest,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create customer response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the customer profile is created successfully")]
        public async Task ThenTheCustomerProfileIsCreatedSuccessfully(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected create customer result table", expected, _apiContext.JsonOptions);

            var customerResponse = await AssertSuccessfulCustomerResponse(expected);
            _customerId = customerResponse.Id;

            AssertIndividual(customerResponse.Individual, expected, "Individual");
            customerResponse.Companies.Count.ShouldBe(ParseInt(expected, "Companies.Count"));
        }

        [Given("I have valid company data for the customer")]
        public void GivenIHaveValidCompanyDataForTheCustomer(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Add company request table", requestValues, _apiContext.JsonOptions);

            _addCompanyRequest = new AddCompanyRequestDto
            {
                TaxId = GetRequiredValue(requestValues, "TaxId"),
                CompanyName = GetRequiredValue(requestValues, "CompanyName"),
                BillingAddress = CreateAddress(requestValues, "BillingAddress"),
                ShippingAddress = CreateAddress(requestValues, "ShippingAddress")
            };

            AllureJson.AttachObject("Add customer company request", _addCompanyRequest, _apiContext.JsonOptions);
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
            AllureJson.AttachRawJson($"Add company response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the customer profile contains the new company data")]
        public async Task ThenTheCustomerProfileContainsTheNewCompanyData(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected add company result table", expected, _apiContext.JsonOptions);

            var customerResponse = await AssertSuccessfulCustomerResponse(expected);
            customerResponse.Id.ShouldBe(_customerId);
            customerResponse.Individual.FirstName.ShouldBe(GetExpectedValue(expected, "Individual.FirstName", customerResponse.Individual.FirstName));
            customerResponse.Individual.LastName.ShouldBe(GetExpectedValue(expected, "Individual.LastName", customerResponse.Individual.LastName));
            customerResponse.Companies.Count.ShouldBe(ParseInt(expected, "Companies.Count"));

            var company = customerResponse.Companies.ShouldHaveSingleItem();
            AssertCompany(company, expected, "Company");
        }

        private async Task<CustomerResponseDto> AssertSuccessfulCustomerResponse(IReadOnlyDictionary<string, string> expected)
        {
            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var customerResponse = await DeserializeResponse<CustomerResponseDto>(_apiContext.Response);
            customerResponse.ShouldNotBeNull();
            AllureJson.AttachObject("Customer response", customerResponse!, _apiContext.JsonOptions);

            if (TryGetBool(expected, "HasId", out var hasId))
            {
                if (hasId)
                {
                    customerResponse!.Id.ShouldNotBe(Guid.Empty);
                }
                else
                {
                    customerResponse!.Id.ShouldBe(Guid.Empty);
                }
            }

            customerResponse!.ExternalId.ShouldBe(GetExpectedValue(expected, "ExternalId", customerResponse.ExternalId));
            return customerResponse;
        }

        private static void AssertIndividual(IndividualDataResponseDto individual, IReadOnlyDictionary<string, string> expected, string prefix)
        {
            individual.FirstName.ShouldBe(GetExpectedValue(expected, $"{prefix}.FirstName", individual.FirstName));
            individual.LastName.ShouldBe(GetExpectedValue(expected, $"{prefix}.LastName", individual.LastName));
            individual.Email.ShouldBe(GetExpectedValue(expected, $"{prefix}.Email", individual.Email));
            individual.Phone.ShouldBe(GetExpectedValue(expected, $"{prefix}.Phone", individual.Phone));
            AssertAddress(individual.BillingAddress, expected, $"{prefix}.BillingAddress");
            AssertAddress(individual.ShippingAddress, expected, $"{prefix}.ShippingAddress");
        }

        private static void AssertCompany(CompanyDataResponseDto company, IReadOnlyDictionary<string, string> expected, string prefix)
        {
            if (TryGetBool(expected, $"{prefix}.HasId", out var hasId))
            {
                if (hasId)
                {
                    company.Id.ShouldNotBe(Guid.Empty);
                }
                else
                {
                    company.Id.ShouldBe(Guid.Empty);
                }
            }

            company.TaxId.ShouldBe(GetExpectedValue(expected, $"{prefix}.TaxId", company.TaxId));
            company.CompanyName.ShouldBe(GetExpectedValue(expected, $"{prefix}.CompanyName", company.CompanyName));
            AssertAddress(company.BillingAddress, expected, $"{prefix}.BillingAddress");
            AssertAddress(company.ShippingAddress, expected, $"{prefix}.ShippingAddress");
        }

        private static void AssertAddress(AddressResponseDto address, IReadOnlyDictionary<string, string> expected, string prefix)
        {
            address.PostalCode.ShouldBe(GetExpectedValue(expected, $"{prefix}.PostalCode", address.PostalCode));
            address.City.ShouldBe(GetExpectedValue(expected, $"{prefix}.City", address.City));
            address.Street.ShouldBe(GetExpectedValue(expected, $"{prefix}.Street", address.Street));
            address.BuildingNumber.ShouldBe(GetExpectedValue(expected, $"{prefix}.BuildingNumber", address.BuildingNumber));
            address.ApartmentNumber.ShouldBe(GetExpectedValue(expected, $"{prefix}.ApartmentNumber", address.ApartmentNumber));
        }

        private static AddressRequestDto CreateAddress(IReadOnlyDictionary<string, string> values, string prefix)
        {
            return new AddressRequestDto
            {
                PostalCode = GetRequiredValue(values, $"{prefix}.PostalCode"),
                City = GetRequiredValue(values, $"{prefix}.City"),
                Street = GetRequiredValue(values, $"{prefix}.Street"),
                BuildingNumber = GetRequiredValue(values, $"{prefix}.BuildingNumber"),
                ApartmentNumber = GetNullableValue(values, $"{prefix}.ApartmentNumber")
            };
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
                throw new InvalidOperationException($"Missing '{key}' value in customer data table.");
            }

            return value;
        }

        private static string? GetNullableValue(IReadOnlyDictionary<string, string> values, string key)
        {
            if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value;
        }

        private static string? GetExpectedValue(IReadOnlyDictionary<string, string> values, string key, string? fallback)
        {
            return values.TryGetValue(key, out var value) ? GetNullableValue(values, key) : fallback;
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

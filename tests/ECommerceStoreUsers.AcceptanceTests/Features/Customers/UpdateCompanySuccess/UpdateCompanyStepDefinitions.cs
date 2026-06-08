using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.UpdateCompanySuccess
{
    [Binding]
    public class UpdateCompanyStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateCustomerRequestDto? _createCustomerRequest;
        private UpdateCompanyRequestDto? _updateCompanyRequest;
        private Guid _customerId;
        private Guid _companyId;

        public UpdateCompanyStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer with company exists for update company request")]
        public async Task GivenACustomerWithCompanyExistsForUpdateCompanyRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update company setup customer request table", requestValues, _apiContext.JsonOptions);

            _createCustomerRequest = new CreateCustomerRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExternalId"),
                Individual = new IndividualDataRequestDto
                {
                    FirstName = GetRequiredValue(requestValues, "Individual.FirstName"),
                    LastName = GetRequiredValue(requestValues, "Individual.LastName"),
                    Email = GetRequiredValue(requestValues, "Individual.Email"),
                    Phone = GetRequiredValue(requestValues, "Individual.Phone"),
                    BillingAddress = BuildAddress(requestValues, "Individual.BillingAddress"),
                    ShippingAddress = BuildAddress(requestValues, "Individual.ShippingAddress")
                },
                Companies =
                [
                    new AddCompanyRequestDto
                    {
                        TaxId = GetRequiredValue(requestValues, "Company.TaxId"),
                        CompanyName = GetRequiredValue(requestValues, "Company.CompanyName"),
                        BillingAddress = BuildAddress(requestValues, "Company.BillingAddress"),
                        ShippingAddress = BuildAddress(requestValues, "Company.ShippingAddress")
                    }
                ]
            };

            AllureJson.AttachObject(
                "Create customer with company setup request contract",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var body = await response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create customer with company setup response contract ({(int)response.StatusCode})", body);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var customerResponse = await DeserializeResponse<CustomerResponseDto>(response);
            customerResponse.ShouldNotBeNull();
            customerResponse!.Id.ShouldNotBe(Guid.Empty);
            customerResponse.Companies.Count.ShouldBe(1);

            _customerId = customerResponse.Id;
            _companyId = customerResponse.Companies.ShouldHaveSingleItem().Id;
            _companyId.ShouldNotBe(Guid.Empty);
        }

        [Given("I have a valid update company request")]
        public void GivenIHaveAValidUpdateCompanyRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update company request table", requestValues, _apiContext.JsonOptions);

            _updateCompanyRequest = new UpdateCompanyRequestDto
            {
                TaxId = GetRequiredValue(requestValues, "Company.TaxId"),
                CompanyName = GetRequiredValue(requestValues, "Company.CompanyName"),
                BillingAddress = BuildAddress(requestValues, "Company.BillingAddress"),
                ShippingAddress = BuildAddress(requestValues, "Company.ShippingAddress")
            };

            AllureJson.AttachObject(
                "Update company request contract",
                _updateCompanyRequest,
                _apiContext.JsonOptions);
        }

        [When("I submit the update company request")]
        public async Task WhenISubmitTheUpdateCompanyRequest()
        {
            _customerId.ShouldNotBe(Guid.Empty);
            _companyId.ShouldNotBe(Guid.Empty);
            _updateCompanyRequest.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PutAsJsonAsync(
                $"/customers/{_customerId}/companies/{_companyId}",
                _updateCompanyRequest,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Update company response contract ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the company is updated successfully")]
        public async Task ThenTheCompanyIsUpdatedSuccessfully(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected update company result table", expected, _apiContext.JsonOptions);

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

            if (TryGetBool(expected, "SameCompanyId", out var sameCompanyId) && sameCompanyId)
            {
                company.Id.ShouldBe(_companyId);
            }

            company.TaxId.ShouldBe(GetExpectedValue(expected, "Company.TaxId", company.TaxId));
            company.CompanyName.ShouldBe(GetExpectedValue(expected, "Company.CompanyName", company.CompanyName));
            AssertAddress(company.BillingAddress, expected, "Company.BillingAddress");
            AssertAddress(company.ShippingAddress, expected, "Company.ShippingAddress");

            AllureJson.AttachObject(
                "Verified update company response contract",
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
                PostalCode = GetRequiredValue(values, $"{prefix}.PostalCode"),
                City = GetRequiredValue(values, $"{prefix}.City"),
                Street = GetRequiredValue(values, $"{prefix}.Street"),
                BuildingNumber = GetRequiredValue(values, $"{prefix}.BuildingNumber"),
                ApartmentNumber = GetRequiredValue(values, $"{prefix}.ApartmentNumber")
            };
        }

        private static void AssertAddress(AddressResponseDto actual, IReadOnlyDictionary<string, string> expected, string prefix)
        {
            actual.PostalCode.ShouldBe(GetExpectedValue(expected, $"{prefix}.PostalCode", actual.PostalCode));
            actual.City.ShouldBe(GetExpectedValue(expected, $"{prefix}.City", actual.City));
            actual.Street.ShouldBe(GetExpectedValue(expected, $"{prefix}.Street", actual.Street));
            actual.BuildingNumber.ShouldBe(GetExpectedValue(expected, $"{prefix}.BuildingNumber", actual.BuildingNumber));
            actual.ApartmentNumber.ShouldBe(GetExpectedValue(expected, $"{prefix}.ApartmentNumber", actual.ApartmentNumber ?? string.Empty));
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
                throw new InvalidOperationException($"Missing '{key}' value in update company data table.");
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

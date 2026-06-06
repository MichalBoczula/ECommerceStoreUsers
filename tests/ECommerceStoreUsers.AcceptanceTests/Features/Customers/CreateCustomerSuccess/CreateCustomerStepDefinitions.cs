using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.CreateCustomerSuccess
{
    [Binding]
    public class CreateCustomerStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateCustomerRequestDto? _request;

        public CreateCustomerStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have a valid create customer request")]
        public void GivenIHaveAValidCreateCustomerRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Create customer request table", requestValues, _apiContext.JsonOptions);

            _request = new CreateCustomerRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExternalId"),
                Individual = new IndividualDataRequestDto
                {
                    FirstName = GetRequiredValue(requestValues, "Individual.FirstName"),
                    LastName = GetRequiredValue(requestValues, "Individual.LastName"),
                    Email = GetRequiredValue(requestValues, "Individual.Email"),
                    Phone = GetRequiredValue(requestValues, "Individual.Phone"),
                    BillingAddress = CreateAddressRequest(requestValues, "Individual.BillingAddress"),
                    ShippingAddress = CreateAddressRequest(requestValues, "Individual.ShippingAddress")
                },
                Companies = CreateCompanyRequests(requestValues)
            };

            AllureJson.AttachObject(
                "Create customer request contract",
                _request,
                _apiContext.JsonOptions);
        }

        [When("I submit the create customer request")]
        public async Task WhenISubmitTheCreateCustomerRequest()
        {
            _request.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _request,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create customer response contract ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the customer profile is created successfully")]
        public async Task ThenTheCustomerProfileIsCreatedSuccessfully(Table table)
        {
            var expected = ParseExpectedTable(table);

            AllureJson.AttachObject("Expected customer response table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var customerResponse = await DeserializeResponse<CustomerResponseDto>(_apiContext.Response);
            customerResponse.ShouldNotBeNull();

            AllureJson.AttachObject(
                "Create customer response dto contract",
                customerResponse!,
                _apiContext.JsonOptions);

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

            if (TryGetBool(expected, "HasUpdatedAt", out var hasUpdatedAt))
            {
                if (hasUpdatedAt)
                {
                    customerResponse!.UpdatedAt.ShouldNotBe(default);
                }
                else
                {
                    customerResponse!.UpdatedAt.ShouldBe(default);
                }
            }

            customerResponse!.ExternalId.ShouldBe(GetExpectedValue(expected, "ExternalId", customerResponse.ExternalId));
            customerResponse.Companies.Count.ShouldBe(ParseInt(expected, "CompaniesCount"));
            AssertIndividual(customerResponse.Individual, expected);
            AssertCompany(customerResponse.Companies, expected);
        }

        private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _apiContext.JsonOptions);
        }

        private static IReadOnlyCollection<AddCompanyRequestDto> CreateCompanyRequests(IReadOnlyDictionary<string, string> values)
        {
            if (!values.ContainsKey("Company.TaxId"))
            {
                return [];
            }

            return
            [
                new AddCompanyRequestDto
                {
                    TaxId = GetRequiredValue(values, "Company.TaxId"),
                    CompanyName = GetRequiredValue(values, "Company.CompanyName"),
                    BillingAddress = CreateAddressRequest(values, "Company.BillingAddress"),
                    ShippingAddress = CreateAddressRequest(values, "Company.ShippingAddress")
                }
            ];
        }

        private static AddressRequestDto CreateAddressRequest(IReadOnlyDictionary<string, string> values, string prefix)
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

        private static void AssertIndividual(IndividualDataResponseDto actual, IReadOnlyDictionary<string, string> expected)
        {
            actual.FirstName.ShouldBe(GetExpectedValue(expected, "Individual.FirstName", actual.FirstName));
            actual.LastName.ShouldBe(GetExpectedValue(expected, "Individual.LastName", actual.LastName));
            actual.Email.ShouldBe(GetExpectedValue(expected, "Individual.Email", actual.Email));
            actual.Phone.ShouldBe(GetExpectedValue(expected, "Individual.Phone", actual.Phone));
            AssertAddress(actual.BillingAddress, expected, "Individual.BillingAddress");
            AssertAddress(actual.ShippingAddress, expected, "Individual.ShippingAddress");
        }

        private static void AssertCompany(IReadOnlyCollection<CompanyDataResponseDto> actual, IReadOnlyDictionary<string, string> expected)
        {
            if (!expected.ContainsKey("Company.TaxId"))
            {
                return;
            }

            var company = actual.ShouldHaveSingleItem();

            if (TryGetBool(expected, "Company.HasId", out var hasCompanyId))
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

            company.TaxId.ShouldBe(GetExpectedValue(expected, "Company.TaxId", company.TaxId));
            company.CompanyName.ShouldBe(GetExpectedValue(expected, "Company.CompanyName", company.CompanyName));
            AssertAddress(company.BillingAddress, expected, "Company.BillingAddress");
            AssertAddress(company.ShippingAddress, expected, "Company.ShippingAddress");
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
                throw new InvalidOperationException($"Missing '{key}' value in customer expected result table.");
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

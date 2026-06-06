using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.GetCustomerSuccess
{
    [Binding]
    public class GetCustomerStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateCustomerRequestDto? _createdCustomerRequest;
        private Guid _createdCustomerId;
        private string? _externalId;

        public GetCustomerStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer profile exists for get customer request")]
        public async Task GivenACustomerProfileExistsForGetCustomerRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Get customer setup request table", requestValues, _apiContext.JsonOptions);

            _createdCustomerRequest = new CreateCustomerRequestDto
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
                }
            };

            AllureJson.AttachObject(
                "Get customer setup create request",
                _createdCustomerRequest,
                _apiContext.JsonOptions);

            var response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _createdCustomerRequest,
                _apiContext.JsonOptions);

            var body = await response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Get customer setup response JSON ({(int)response.StatusCode})", body);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var customerResponse = await DeserializeResponse<CustomerResponseDto>(response);
            customerResponse.ShouldNotBeNull();
            customerResponse!.Id.ShouldNotBe(Guid.Empty);

            _createdCustomerId = customerResponse.Id;
        }

        [When("I request the customer profile by external id")]
        public async Task WhenIRequestTheCustomerProfileByExternalId(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Get customer request table", requestValues, _apiContext.JsonOptions);

            _externalId = GetRequiredValue(requestValues, "ExternalId");

            _apiContext.Response = await _apiContext.HttpClient.GetAsync(
                $"/customers/external/{Uri.EscapeDataString(_externalId)}");

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Get customer response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the customer profile is returned successfully")]
        public async Task ThenTheCustomerProfileIsReturnedSuccessfully(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected get customer result table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var customerResponse = await DeserializeResponse<CustomerResponseDto>(_apiContext.Response);
            customerResponse.ShouldNotBeNull();

            AllureJson.AttachObject(
                "Get customer response dto contract",
                customerResponse!,
                _apiContext.JsonOptions);

            if (TryGetBool(expected, "HasId", out var hasId))
            {
                if (hasId)
                {
                    customerResponse!.Id.ShouldNotBe(Guid.Empty);
                    customerResponse.Id.ShouldBe(_createdCustomerId);
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
        }

        private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _apiContext.JsonOptions);
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
                throw new InvalidOperationException($"Missing '{key}' value in customer get data table.");
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

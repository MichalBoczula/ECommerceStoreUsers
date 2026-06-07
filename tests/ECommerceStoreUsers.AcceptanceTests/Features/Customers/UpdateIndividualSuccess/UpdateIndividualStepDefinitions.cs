using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.UpdateIndividualSuccess
{
    [Binding]
    public class UpdateIndividualStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateCustomerRequestDto? _createCustomerRequest;
        private UpdateIndividualDataRequestDto? _updateIndividualRequest;
        private Guid _customerId;

        public UpdateIndividualStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer exists for update individual request")]
        public async Task GivenACustomerExistsForUpdateIndividualRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update individual setup customer request table", requestValues, _apiContext.JsonOptions);

            _createCustomerRequest = new CreateCustomerRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExternalId"),
                Individual = BuildIndividualData(requestValues)
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

        [Given("I have a valid update individual request")]
        public void GivenIHaveAValidUpdateIndividualRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update individual request table", requestValues, _apiContext.JsonOptions);

            _updateIndividualRequest = new UpdateIndividualDataRequestDto
            {
                Individual = BuildIndividualData(requestValues)
            };

            AllureJson.AttachObject(
                "Update individual request contract",
                _updateIndividualRequest,
                _apiContext.JsonOptions);
        }

        [When("I submit the update individual request")]
        public async Task WhenISubmitTheUpdateIndividualRequest()
        {
            _customerId.ShouldNotBe(Guid.Empty);
            _updateIndividualRequest.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PutAsJsonAsync(
                $"/customers/{_customerId}/individual",
                _updateIndividualRequest,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Update individual response contract ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the individual data is updated successfully")]
        public async Task ThenTheIndividualDataIsUpdatedSuccessfully(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected update individual result table", expected, _apiContext.JsonOptions);

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
            AssertIndividual(customerResponse.Individual, expected);

            AllureJson.AttachObject(
                "Verified update individual response contract",
                customerResponse,
                _apiContext.JsonOptions);
        }

        private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _apiContext.JsonOptions);
        }

        private static IndividualDataRequestDto BuildIndividualData(IReadOnlyDictionary<string, string> values)
        {
            return new IndividualDataRequestDto
            {
                FirstName = GetRequiredValue(values, "Individual.FirstName"),
                LastName = GetRequiredValue(values, "Individual.LastName"),
                Email = GetRequiredValue(values, "Individual.Email"),
                Phone = GetRequiredValue(values, "Individual.Phone"),
                BillingAddress = BuildAddress(values, "Individual.BillingAddress"),
                ShippingAddress = BuildAddress(values, "Individual.ShippingAddress")
            };
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
            actual.ApartmentNumber.ShouldBe(GetExpectedValue(expected, $"{prefix}.ApartmentNumber", actual.ApartmentNumber!));
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
                throw new InvalidOperationException($"Missing '{key}' value in update individual data table.");
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

using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.CreateCustomerValidationError
{
    [Binding]
    public class CreateCustomerValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateCustomerRequestDto? _request;

        public CreateCustomerValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have an invalid individual create customer request")]
        public void GivenIHaveAnInvalidIndividualCreateCustomerRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Create individual customer invalid request table", requestValues, _apiContext.JsonOptions);

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
                }
            };

            AllureJson.AttachObject(
                "Create individual customer invalid request payload",
                _request,
                _apiContext.JsonOptions);
        }

        [When("I submit the create customer request for validation")]
        public async Task WhenISubmitTheCreateCustomerRequestForValidation()
        {
            _request.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _request,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create Customer Validation Response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the customer profile creation fails with a validation error")]
        public async Task ThenTheCustomerProfileCreationFailsWithAValidationError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected create customer validation response table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<ApiProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();
            problemDetails.Errors.ShouldNotBeEmpty();

            var expectedMessage = GetRequiredValue(expected, "Message");
            var expectedName = GetRequiredValue(expected, "Name");
            var expectedEntity = GetRequiredValue(expected, "Entity");

            var addressError = problemDetails.Errors.FirstOrDefault(e => e.Name == expectedName);

            addressError.ShouldNotBeNull($"Expected validation rule '{expectedName}' was not triggered.");
            addressError.Message.ShouldBe(expectedMessage);
            addressError.Entity.ShouldBe(expectedEntity);

            problemDetails.TraceId.ShouldNotBeNullOrWhiteSpace();
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

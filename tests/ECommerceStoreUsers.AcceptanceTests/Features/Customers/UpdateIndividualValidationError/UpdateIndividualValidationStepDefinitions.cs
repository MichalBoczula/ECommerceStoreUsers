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

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.UpdateIndividualValidationError
{
    [Binding]
    public class UpdateIndividualValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateCustomerRequestDto? _createCustomerRequest;
        private UpdateIndividualDataRequestDto? _updateIndividualRequest;
        private Guid _customerId;

        public UpdateIndividualValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer exists for update individual validation request")]
        public async Task GivenACustomerExistsForUpdateIndividualValidationRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update individual validation setup customer request table", requestValues, _apiContext.JsonOptions);

            _createCustomerRequest = new CreateCustomerRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExternalId"),
                Individual = BuildIndividualData(requestValues)
            };

            AllureJson.AttachObject(
                "Create customer setup request contract for update individual validation",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var body = await response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create customer setup response contract for update individual validation ({(int)response.StatusCode})", body);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var customerResponse = await DeserializeResponse<CustomerResponseDto>(response);
            customerResponse.ShouldNotBeNull();
            customerResponse!.Id.ShouldNotBe(Guid.Empty);

            _customerId = customerResponse.Id;
        }

        [Given("I have an invalid update individual request")]
        public void GivenIHaveAnInvalidUpdateIndividualRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Invalid update individual request table", requestValues, _apiContext.JsonOptions);

            _updateIndividualRequest = new UpdateIndividualDataRequestDto
            {
                Individual = BuildIndividualData(requestValues)
            };

            AllureJson.AttachObject(
                "Invalid update individual request contract",
                _updateIndividualRequest,
                _apiContext.JsonOptions);
        }

        [When("I submit the update individual request for validation")]
        public async Task WhenISubmitTheUpdateIndividualRequestForValidation()
        {
            _customerId.ShouldNotBe(Guid.Empty);
            _updateIndividualRequest.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PutAsJsonAsync(
                $"/customers/{_customerId}/individual",
                _updateIndividualRequest,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Update individual validation response contract ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("updating the individual data fails with validation errors")]
        public async Task ThenUpdatingTheIndividualDataFailsWithValidationErrors(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected update individual validation result table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<ApiProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();
            problemDetails.Errors.ShouldNotBeEmpty();

            AssertValidationError(
                problemDetails,
                GetRequiredValue(expected, "EmailName"),
                GetRequiredValue(expected, "EmailMessage"),
                GetRequiredValue(expected, "EmailEntity"));

            AssertValidationError(
                problemDetails,
                GetRequiredValue(expected, "PhoneName"),
                GetRequiredValue(expected, "PhoneMessage"),
                GetRequiredValue(expected, "PhoneEntity"));

            problemDetails.TraceId.ShouldNotBeNullOrWhiteSpace();
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
                FirstName = GetRequiredValue(values, "FirstName"),
                LastName = GetRequiredValue(values, "LastName"),
                Email = GetRequiredValue(values, "Email"),
                Phone = GetRequiredValue(values, "Phone"),
                BillingAddress = BuildAddress(values, "Billing"),
                ShippingAddress = BuildAddress(values, "Shipping")
            };
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

        private static void AssertValidationError(
            ApiProblemDetails problemDetails,
            string expectedName,
            string expectedMessage,
            string expectedEntity)
        {
            var validationError = problemDetails.Errors.FirstOrDefault(e => e.Name == expectedName);

            validationError.ShouldNotBeNull($"Expected validation rule '{expectedName}' was not triggered.");
            validationError.Message.ShouldBe(expectedMessage);
            validationError.Entity.ShouldBe(expectedEntity);
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

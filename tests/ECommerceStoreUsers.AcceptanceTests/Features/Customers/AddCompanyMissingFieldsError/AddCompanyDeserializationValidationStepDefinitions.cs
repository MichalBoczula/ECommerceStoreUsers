using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using Microsoft.AspNetCore.Mvc;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.AddCompanyMissingFieldsError
{
    [Binding]
    public class AddCompanyDeserializationValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private Guid _customerId;
        private Dictionary<string, object>? _rawPayload;

        public AddCompanyDeserializationValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer exists for add company missing fields request")]
        public async Task GivenACustomerExistsForAddCompanyMissingFieldsRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Add company missing fields customer setup table", requestValues, _apiContext.JsonOptions);

            var createRequest = new CreateCustomerRequestDto
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
                "Create customer setup payload for add company missing fields flow",
                createRequest,
                _apiContext.JsonOptions);

            var createResponse = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                createRequest,
                _apiContext.JsonOptions);

            var createBody = await createResponse.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create customer setup response JSON ({(int)createResponse.StatusCode})", createBody);

            createResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var createdCustomer = JsonSerializer.Deserialize<CustomerResponseDto>(createBody, _apiContext.JsonOptions);
            createdCustomer.ShouldNotBeNull();
            createdCustomer!.Id.ShouldNotBe(Guid.Empty);
            _customerId = createdCustomer.Id;
        }

        [Given("I have an add company request payload missing the company name")]
        public void GivenIHaveAnAddCompanyRequestPayloadMissingTheCompanyName(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Incomplete add company request table", requestValues, _apiContext.JsonOptions);

            _rawPayload = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["taxId"] = GetRequiredValue(requestValues, "TaxId"),
                ["billingAddress"] = BuildRawAddress(requestValues, "Billing"),
                ["shippingAddress"] = BuildRawAddress(requestValues, "Shipping")
            };

            AllureJson.AttachObject(
                "Incomplete add company raw request JSON dictionary structure",
                _rawPayload,
                _apiContext.JsonOptions);
        }

        [When("I submit the incomplete add company request")]
        public async Task WhenISubmitTheIncompleteAddCompanyRequest()
        {
            _customerId.ShouldNotBe(Guid.Empty);
            _rawPayload.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                $"/customers/{_customerId}/companies",
                _rawPayload,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Add company deserialization exception response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the add company response indicates a json deserialization failure")]
        public async Task ThenTheAddCompanyResponseIndicatesAJsonDeserializationFailure(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected add company deserialization error constraints", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<ProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();
            problemDetails!.Status.ShouldBe((int)ParseStatusCode(expected, "StatusCode"));
            problemDetails.Title.ShouldBe(GetRequiredValue(expected, "Title"));
            problemDetails.Detail.ShouldBe(GetRequiredValue(expected, "Detail"));
            problemDetails.Type.ShouldBe(GetRequiredValue(expected, "Type"));
            problemDetails.Instance.ShouldBe($"/customers/{_customerId}/companies");
            problemDetails.Extensions.TryGetValue("traceId", out var traceId).ShouldBeTrue();
            traceId.ShouldNotBeNull();
            problemDetails.Detail.ShouldContain(GetRequiredValue(expected, "MissingProperty"));
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

        private static Dictionary<string, string> BuildRawAddress(IReadOnlyDictionary<string, string> values, string prefix)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["postalCode"] = GetRequiredValue(values, $"{prefix}PostalCode"),
                ["city"] = GetRequiredValue(values, $"{prefix}City"),
                ["street"] = GetRequiredValue(values, $"{prefix}Street"),
                ["buildingNumber"] = GetRequiredValue(values, $"{prefix}BuildingNumber"),
                ["apartmentNumber"] = GetRequiredValue(values, $"{prefix}ApartmentNumber")
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
                throw new InvalidOperationException($"Missing '{key}' value in add company missing fields data table.");
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

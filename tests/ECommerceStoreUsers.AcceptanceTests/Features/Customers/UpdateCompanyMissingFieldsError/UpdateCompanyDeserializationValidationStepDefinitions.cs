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

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.UpdateCompanyMissingFieldsError
{
    [Binding]
    public class UpdateCompanyDeserializationValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private Guid _customerId;
        private Guid _companyId;
        private Dictionary<string, object?>? _rawPayload;

        public UpdateCompanyDeserializationValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer with a company exists for update company missing fields request")]
        public async Task GivenACustomerWithACompanyExistsForUpdateCompanyMissingFieldsRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update company missing fields customer setup table", requestValues, _apiContext.JsonOptions);

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
                "Create customer with company setup payload for update company missing fields flow",
                createRequest,
                _apiContext.JsonOptions);

            var createResponse = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                createRequest,
                _apiContext.JsonOptions);

            var createBody = await createResponse.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create customer with company setup response JSON ({(int)createResponse.StatusCode})", createBody);

            createResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var createdCustomer = JsonSerializer.Deserialize<CustomerResponseDto>(createBody, _apiContext.JsonOptions);
            createdCustomer.ShouldNotBeNull();
            createdCustomer!.Id.ShouldNotBe(Guid.Empty);
            createdCustomer.Companies.ShouldHaveSingleItem();

            _customerId = createdCustomer.Id;
            _companyId = createdCustomer.Companies.Single().Id;
            _companyId.ShouldNotBe(Guid.Empty);
        }

        [Given("I have an update company request payload missing the company name and shipping address")]
        public void GivenIHaveAnUpdateCompanyRequestPayloadMissingTheCompanyNameAndShippingAddress(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Incomplete update company request table", requestValues, _apiContext.JsonOptions);

            _rawPayload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["taxId"] = GetRequiredValue(requestValues, "TaxId"),
                ["billingAddress"] = BuildRawAddress(requestValues, "Billing")
            };

            AllureJson.AttachObject(
                "Incomplete update company raw request JSON dictionary structure",
                _rawPayload,
                _apiContext.JsonOptions);
        }

        [When("I submit the incomplete update company request")]
        public async Task WhenISubmitTheIncompleteUpdateCompanyRequest()
        {
            _customerId.ShouldNotBe(Guid.Empty);
            _companyId.ShouldNotBe(Guid.Empty);
            _rawPayload.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PutAsJsonAsync(
                $"/customers/{_customerId}/companies/{_companyId}",
                _rawPayload,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Update company deserialization exception response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the update company response indicates a json deserialization failure")]
        public async Task ThenTheUpdateCompanyResponseIndicatesAJsonDeserializationFailure(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected update company deserialization error constraints", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<ProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();
            problemDetails!.Status.ShouldBe((int)ParseStatusCode(expected, "StatusCode"));
            problemDetails.Title.ShouldBe(GetRequiredValue(expected, "Title"));
            problemDetails.Detail.ShouldBe(GetRequiredValue(expected, "Detail"));
            problemDetails.Type.ShouldBe(GetRequiredValue(expected, "Type"));
            problemDetails.Instance.ShouldBe($"/customers/{_customerId}/companies/{_companyId}");
            problemDetails.Extensions.TryGetValue("traceId", out var traceId).ShouldBeTrue();
            traceId.ShouldNotBeNull();

            foreach (var missingProperty in GetRequiredValues(expected, "MissingProperty"))
            {
                problemDetails.Detail.ShouldContain(missingProperty);
            }
        }

        private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _apiContext.JsonOptions);
        }

        private static AddressRequestDto BuildAddress(IReadOnlyDictionary<string, List<string>> values, string prefix)
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

        private static Dictionary<string, object?> BuildRawAddress(IReadOnlyDictionary<string, List<string>> values, string prefix)
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["postalCode"] = GetRequiredValue(values, $"{prefix}PostalCode"),
                ["city"] = GetRequiredValue(values, $"{prefix}City"),
                ["street"] = GetRequiredValue(values, $"{prefix}Street"),
                ["buildingNumber"] = GetRequiredValue(values, $"{prefix}BuildingNumber"),
                ["apartmentNumber"] = GetRequiredValue(values, $"{prefix}ApartmentNumber")
            };
        }

        private static Dictionary<string, List<string>> ParseExpectedTable(Table table)
        {
            var values = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in table.Rows)
            {
                if (!values.TryGetValue(row["Field"], out var fieldValues))
                {
                    fieldValues = [];
                    values[row["Field"]] = fieldValues;
                }

                fieldValues.Add(row["Value"]);
            }

            return values;
        }

        private static string GetRequiredValue(IReadOnlyDictionary<string, List<string>> values, string key)
        {
            return GetRequiredValues(values, key).Single();
        }

        private static IReadOnlyCollection<string> GetRequiredValues(IReadOnlyDictionary<string, List<string>> values, string key)
        {
            if (!values.TryGetValue(key, out var value) || value.Count == 0)
            {
                throw new InvalidOperationException($"Missing '{key}' value in update company missing fields data table.");
            }

            return value;
        }

        private static HttpStatusCode ParseStatusCode(IReadOnlyDictionary<string, List<string>> values, string key)
        {
            var value = GetRequiredValue(values, key);
            return (HttpStatusCode)int.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}

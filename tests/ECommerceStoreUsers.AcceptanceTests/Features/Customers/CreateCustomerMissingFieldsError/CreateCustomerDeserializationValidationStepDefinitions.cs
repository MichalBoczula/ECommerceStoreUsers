using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using Microsoft.AspNetCore.Mvc;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.CreateCustomerMissingFieldsError
{
    [Binding]
    public class CreateCustomerDeserializationValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private Dictionary<string, object?>? _rawPayload;

        public CreateCustomerDeserializationValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have a create customer request payload missing the individual email and phone")]
        public void GivenIHaveACreateCustomerRequestPayloadMissingTheIndividualEmailAndPhone(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Create customer missing individual fields setup table", requestValues, _apiContext.JsonOptions);

            _rawPayload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["ExternalId"] = GetRequiredValue(requestValues, "ExternalId"),
                ["Individual"] = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["FirstName"] = GetRequiredValue(requestValues, "Individual.FirstName"),
                    ["LastName"] = GetRequiredValue(requestValues, "Individual.LastName"),
                    ["BillingAddress"] = CreateAddressPayload(requestValues, "Individual.BillingAddress"),
                    ["ShippingAddress"] = CreateAddressPayload(requestValues, "Individual.ShippingAddress")
                },
                ["Companies"] = Array.Empty<object>()
            };

            AllureJson.AttachObject(
                "Incomplete create customer raw request JSON dictionary structure",
                _rawPayload,
                _apiContext.JsonOptions);
        }

        [When("I submit the incomplete create customer request")]
        public async Task WhenISubmitTheIncompleteCreateCustomerRequest()
        {
            _rawPayload.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _rawPayload,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create customer deserialization exception response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the create customer response indicates a json deserialization failure")]
        public async Task ThenTheCreateCustomerResponseIndicatesAJsonDeserializationFailure(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected create customer deserialization error constraints", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<ProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();
            problemDetails!.Status.ShouldBe((int)ParseStatusCode(expected, "StatusCode"));
            problemDetails.Title.ShouldBe(GetRequiredValue(expected, "Title"));
            problemDetails.Detail.ShouldBe(GetRequiredValue(expected, "Detail"));
            problemDetails.Type.ShouldBe(GetRequiredValue(expected, "Type"));
            problemDetails.Instance.ShouldBe("/customers");
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

        private static Dictionary<string, object?> CreateAddressPayload(IReadOnlyDictionary<string, List<string>> values, string prefix)
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["PostalCode"] = GetRequiredValue(values, $"{prefix}.PostalCode"),
                ["City"] = GetRequiredValue(values, $"{prefix}.City"),
                ["Street"] = GetRequiredValue(values, $"{prefix}.Street"),
                ["BuildingNumber"] = GetRequiredValue(values, $"{prefix}.BuildingNumber"),
                ["ApartmentNumber"] = GetRequiredValue(values, $"{prefix}.ApartmentNumber")
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
                throw new InvalidOperationException($"Missing '{key}' value in create customer missing fields data table.");
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

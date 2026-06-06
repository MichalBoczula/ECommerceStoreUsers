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

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.AddCompanyConflictError
{
    [Binding]
    public class AddCompanyConflictStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private Guid _customerId;
        private AddCompanyRequestDto? _request;

        public AddCompanyConflictStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer with a company exists for add company conflict request")]
        public async Task GivenACustomerWithACompanyExistsForAddCompanyConflictRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Add company conflict customer setup table", requestValues, _apiContext.JsonOptions);

            var createRequest = new CreateCustomerRequestDto
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
                Companies = new[]
                {
                    new AddCompanyRequestDto
                    {
                        TaxId = GetRequiredValue(requestValues, "ExistingCompany.TaxId"),
                        CompanyName = GetRequiredValue(requestValues, "ExistingCompany.CompanyName"),
                        BillingAddress = BuildAddress(requestValues, "ExistingCompany.BillingAddress"),
                        ShippingAddress = BuildAddress(requestValues, "ExistingCompany.ShippingAddress")
                    }
                }
            };

            AllureJson.AttachObject(
                "Create customer setup payload for add company conflict flow",
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
            createdCustomer.Companies.ShouldContain(company => company.TaxId == GetRequiredValue(requestValues, "ExistingCompany.TaxId"));
            _customerId = createdCustomer.Id;
        }

        [Given("I have a duplicate tax id add company request")]
        public void GivenIHaveADuplicateTaxIdAddCompanyRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Duplicate tax id add company request table", requestValues, _apiContext.JsonOptions);

            _request = new AddCompanyRequestDto
            {
                TaxId = GetRequiredValue(requestValues, "TaxId"),
                CompanyName = GetRequiredValue(requestValues, "CompanyName"),
                BillingAddress = BuildAddress(requestValues, "BillingAddress"),
                ShippingAddress = BuildAddress(requestValues, "ShippingAddress")
            };

            AllureJson.AttachObject(
                "Duplicate tax id add company request contract",
                _request,
                _apiContext.JsonOptions);
        }

        [When("I submit the duplicate tax id add company request")]
        public async Task WhenISubmitTheDuplicateTaxIdAddCompanyRequest()
        {
            _customerId.ShouldNotBe(Guid.Empty);
            _request.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                $"/customers/{_customerId}/companies",
                _request,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Add company conflict response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the add company request fails with a conflict error")]
        public async Task ThenTheAddCompanyRequestFailsWithAConflictError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected add company conflict table results", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var conflictProblemDetails = await DeserializeResponse<ConflictProblemDetails>(_apiContext.Response);
            conflictProblemDetails.ShouldNotBeNull();

            AllureJson.AttachObject("Add company conflict response problem details", conflictProblemDetails!, _apiContext.JsonOptions);

            conflictProblemDetails!.Status.ShouldBe(ParseInt(expected, "Status"));
            conflictProblemDetails.Title.ShouldBe(GetRequiredValue(expected, "Title"));
            conflictProblemDetails.Type.ShouldBe(GetRequiredValue(expected, "Type"));
            conflictProblemDetails.Instance.ShouldBe($"/customers/{_customerId}/companies");
            conflictProblemDetails.Detail.ShouldContain(GetRequiredValue(expected, "DetailContains"));
            conflictProblemDetails.Detail.ShouldContain(GetRequiredValue(expected, "DuplicatedTaxId"));

            if (TryGetBool(expected, "HasTraceId", out var hasTraceId))
            {
                if (hasTraceId)
                {
                    conflictProblemDetails.TraceId.ShouldNotBeNullOrWhiteSpace();
                }
                else
                {
                    conflictProblemDetails.TraceId.ShouldBeNullOrWhiteSpace();
                }
            }
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
                throw new InvalidOperationException($"Missing '{key}' key in add company conflict data table mapping.");
            }

            return value;
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

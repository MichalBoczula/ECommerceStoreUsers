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

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.UpdateCompanyConflictError
{
    [Binding]
    public class UpdateCompanyConflictStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private Guid _customerId;
        private Guid _companyId;
        private UpdateCompanyRequestDto? _request;

        public UpdateCompanyConflictStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer with two companies exists for update company conflict request")]
        public async Task GivenACustomerWithTwoCompaniesExistsForUpdateCompanyConflictRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update company conflict customer setup table", requestValues, _apiContext.JsonOptions);

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
                    BuildCompany(requestValues, "FirstCompany"),
                    BuildCompany(requestValues, "SecondCompany")
                }
            };

            AllureJson.AttachObject(
                "Create customer setup payload for update company conflict flow",
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
            createdCustomer.Companies.Count.ShouldBe(2);

            _customerId = createdCustomer.Id;
            _companyId = ResolveCompanyId(createdCustomer, requestValues);
            _companyId.ShouldNotBe(Guid.Empty);

            AllureJson.AttachObject(
                "Resolved update company conflict identifiers",
                new { CustomerId = _customerId, CompanyId = _companyId },
                _apiContext.JsonOptions);
        }

        [Given("I have a duplicate tax id update company request")]
        public void GivenIHaveADuplicateTaxIdUpdateCompanyRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Duplicate tax id update company request table", requestValues, _apiContext.JsonOptions);

            _request = new UpdateCompanyRequestDto
            {
                TaxId = GetRequiredValue(requestValues, "TaxId"),
                CompanyName = GetRequiredValue(requestValues, "CompanyName"),
                BillingAddress = BuildAddress(requestValues, "BillingAddress"),
                ShippingAddress = BuildAddress(requestValues, "ShippingAddress")
            };

            AllureJson.AttachObject(
                "Duplicate tax id update company request contract",
                _request,
                _apiContext.JsonOptions);
        }

        [When("I submit the duplicate tax id update company request")]
        public async Task WhenISubmitTheDuplicateTaxIdUpdateCompanyRequest()
        {
            _customerId.ShouldNotBe(Guid.Empty);
            _companyId.ShouldNotBe(Guid.Empty);
            _request.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PutAsJsonAsync(
                $"/customers/{_customerId}/companies/{_companyId}",
                _request,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Update company conflict response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the update company request fails with a conflict error")]
        public async Task ThenTheUpdateCompanyRequestFailsWithAConflictError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected update company conflict table results", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var conflictProblemDetails = await DeserializeResponse<ConflictProblemDetails>(_apiContext.Response);
            conflictProblemDetails.ShouldNotBeNull();

            AllureJson.AttachObject("Update company conflict response problem details", conflictProblemDetails!, _apiContext.JsonOptions);

            conflictProblemDetails!.Status.ShouldBe(ParseInt(expected, "Status"));
            conflictProblemDetails.Title.ShouldBe(GetRequiredValue(expected, "Title"));
            conflictProblemDetails.Type.ShouldBe(GetRequiredValue(expected, "Type"));
            conflictProblemDetails.Instance.ShouldBe($"/customers/{_customerId}/companies/{_companyId}");
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

        private static AddCompanyRequestDto BuildCompany(IReadOnlyDictionary<string, string> values, string prefix)
        {
            return new AddCompanyRequestDto
            {
                TaxId = GetRequiredValue(values, $"{prefix}.TaxId"),
                CompanyName = GetRequiredValue(values, $"{prefix}.CompanyName"),
                BillingAddress = BuildAddress(values, $"{prefix}.BillingAddress"),
                ShippingAddress = BuildAddress(values, $"{prefix}.ShippingAddress")
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

        private static Guid ResolveCompanyId(CustomerResponseDto createdCustomer, IReadOnlyDictionary<string, string> values)
        {
            var companyKey = GetRequiredValue(values, "CompanyToUpdate");
            var taxId = GetRequiredValue(values, $"{companyKey}.TaxId");
            var company = createdCustomer.Companies.Single(company => company.TaxId == taxId);

            return company.Id;
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
                throw new InvalidOperationException($"Missing '{key}' key in update company conflict data table mapping.");
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

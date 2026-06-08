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

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.UpdateCompanyNotFound
{
    [Binding]
    public class UpdateCompanyNotFoundStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private Guid _customerId;
        private Guid? _companyId;
        private CreateCustomerRequestDto? _createCustomerRequest;
        private UpdateCompanyRequestDto? _updateCompanyRequest;

        public UpdateCompanyNotFoundStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("a customer with a company exists for update company not found request")]
        public async Task GivenACustomerWithACompanyExistsForUpdateCompanyNotFoundRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update missing company setup table", requestValues, _apiContext.JsonOptions);

            _createCustomerRequest = new CreateCustomerRequestDto
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
                Companies =
                [
                    new AddCompanyRequestDto
                    {
                        TaxId = GetRequiredValue(requestValues, "ExistingCompany.TaxId"),
                        CompanyName = GetRequiredValue(requestValues, "ExistingCompany.CompanyName"),
                        BillingAddress = BuildAddress(requestValues, "ExistingCompany.BillingAddress"),
                        ShippingAddress = BuildAddress(requestValues, "ExistingCompany.ShippingAddress")
                    }
                ]
            };

            AllureJson.AttachObject(
                "Update missing company setup create customer request contract",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _createCustomerRequest,
                _apiContext.JsonOptions);

            var body = await response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Update missing company setup create customer response JSON ({(int)response.StatusCode})", body);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var customerResponse = await DeserializeResponse<CustomerResponseDto>(response);
            customerResponse.ShouldNotBeNull();
            customerResponse!.Id.ShouldNotBe(Guid.Empty);
            customerResponse.Companies.Count.ShouldBe(1);

            _customerId = customerResponse.Id;

            AllureJson.AttachObject(
                "Update missing company setup persisted customer contract",
                customerResponse,
                _apiContext.JsonOptions);
        }

        [Given("I have an update missing company request")]
        public void GivenIHaveAnUpdateMissingCompanyRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update missing company request table", requestValues, _apiContext.JsonOptions);

            _companyId = Guid.Parse(GetRequiredValue(requestValues, "CompanyId"));
            _updateCompanyRequest = new UpdateCompanyRequestDto
            {
                TaxId = GetRequiredValue(requestValues, "TaxId"),
                CompanyName = GetRequiredValue(requestValues, "CompanyName"),
                BillingAddress = BuildAddress(requestValues, "BillingAddress"),
                ShippingAddress = BuildAddress(requestValues, "ShippingAddress")
            };

            AllureJson.AttachObject(
                "Update missing company request contract",
                new { CustomerId = _customerId, CompanyId = _companyId, Request = _updateCompanyRequest },
                _apiContext.JsonOptions);
        }

        [When("I submit the update missing company request")]
        public async Task WhenISubmitTheUpdateMissingCompanyRequest()
        {
            _customerId.ShouldNotBe(Guid.Empty);
            _companyId.ShouldNotBeNull();
            _updateCompanyRequest.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PutAsJsonAsync(
                $"/customers/{_customerId}/companies/{_companyId}",
                _updateCompanyRequest,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Update missing company not found response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the update company request fails with a not found response")]
        public async Task ThenTheUpdateCompanyRequestFailsWithANotFoundResponse(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected update missing company not found result table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<NotFoundProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();

            AllureJson.AttachObject(
                "Verified update missing company not found problem details",
                problemDetails!,
                _apiContext.JsonOptions);

            problemDetails!.Status.ShouldBe((int)ParseStatusCode(expected, "StatusCode"));
            problemDetails.Title.ShouldBe(GetRequiredValue(expected, "Title"));
            problemDetails.Detail.ShouldBe(ResolveExpectedValue(GetRequiredValue(expected, "Detail")));
            problemDetails.Type.ShouldBe(GetRequiredValue(expected, "Type"));
            problemDetails.Instance.ShouldBe(ResolveExpectedValue(GetRequiredValue(expected, "InstanceTemplate")));

            if (TryGetBool(expected, "HasTraceId", out var hasTraceId))
            {
                if (hasTraceId)
                {
                    problemDetails.TraceId.ShouldNotBeNullOrWhiteSpace();
                }
                else
                {
                    problemDetails.TraceId.ShouldBeNullOrWhiteSpace();
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

        private string ResolveExpectedValue(string value)
        {
            _companyId.ShouldNotBeNull();

            return value
                .Replace("{CustomerId}", _customerId.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{CompanyId}", _companyId.Value.ToString(), StringComparison.OrdinalIgnoreCase);
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
                throw new InvalidOperationException($"Missing '{key}' update company not found key value in data table.");
            }

            return value;
        }

        private static HttpStatusCode ParseStatusCode(IReadOnlyDictionary<string, string> values, string key)
        {
            var value = GetRequiredValue(values, key);
            return (HttpStatusCode)int.Parse(value, CultureInfo.InvariantCulture);
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

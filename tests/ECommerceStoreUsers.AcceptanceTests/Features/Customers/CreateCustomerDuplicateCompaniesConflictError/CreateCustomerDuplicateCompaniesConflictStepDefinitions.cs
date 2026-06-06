using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.CreateCustomerDuplicateCompaniesConflictError
{
    [Binding]
    public class CreateCustomerDuplicateCompaniesConflictStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateCustomerRequestDto? _request;

        public CreateCustomerDuplicateCompaniesConflictStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have a create customer duplicate companies request payload")]
        public void GivenIHaveACreateCustomerDuplicateCompaniesRequestPayload(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Create customer duplicate companies request data table", requestValues, _apiContext.JsonOptions);

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

            AllureJson.AttachObject("Create customer duplicate companies base request contract", _request, _apiContext.JsonOptions);
        }

        [Given("the duplicate companies are included in the create customer payload")]
        public void GivenTheDuplicateCompaniesAreIncludedInTheCreateCustomerPayload(Table table)
        {
            _request.ShouldNotBeNull();

            var companies = table.Rows
                .Select(row => new AddCompanyRequestDto
                {
                    CompanyName = GetRequiredValue(row, "CompanyName"),
                    TaxId = GetRequiredValue(row, "TaxId"),
                    BillingAddress = CreateAddressRequest(row, "BillingAddress"),
                    ShippingAddress = CreateAddressRequest(row, "ShippingAddress")
                })
                .ToArray();

            AllureJson.AttachObject("Create customer duplicate companies table", companies, _apiContext.JsonOptions);

            _request = _request with
            {
                Companies = companies
            };

            AllureJson.AttachObject("Create customer duplicate companies request contract", _request, _apiContext.JsonOptions);
        }

        [When("I submit the create customer duplicate companies request")]
        public async Task WhenISubmitTheCreateCustomerDuplicateCompaniesRequest()
        {
            _request.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/customers",
                _request,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Create customer duplicate companies response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the create customer duplicate companies request fails with a conflict error")]
        public async Task ThenTheCreateCustomerDuplicateCompaniesRequestFailsWithAConflictError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected create customer duplicate companies conflict table results", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var conflictProblemDetails = await DeserializeResponse<ConflictProblemDetails>(_apiContext.Response);
            conflictProblemDetails.ShouldNotBeNull();

            AllureJson.AttachObject("Create customer duplicate companies conflict response problem details", conflictProblemDetails!, _apiContext.JsonOptions);

            conflictProblemDetails!.Status.ShouldBe(ParseInt(expected, "Status"));
            conflictProblemDetails.Title.ShouldBe(GetRequiredValue(expected, "Title"));
            conflictProblemDetails.Type.ShouldBe(GetRequiredValue(expected, "Type"));
            conflictProblemDetails.Instance.ShouldBe(GetRequiredValue(expected, "Instance"));
            conflictProblemDetails.Detail.ShouldContain(GetRequiredValue(expected, "DetailContains"));

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

        private static AddressRequestDto CreateAddressRequest(TableRow row, string prefix)
        {
            return new AddressRequestDto
            {
                PostalCode = GetRequiredValue(row, $"{prefix}.PostalCode"),
                City = GetRequiredValue(row, $"{prefix}.City"),
                Street = GetRequiredValue(row, $"{prefix}.Street"),
                BuildingNumber = GetRequiredValue(row, $"{prefix}.BuildingNumber"),
                ApartmentNumber = GetRequiredValue(row, $"{prefix}.ApartmentNumber")
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
                throw new InvalidOperationException($"Missing '{key}' key in create customer duplicate companies data table mapping.");
            }

            return value;
        }

        private static string GetRequiredValue(TableRow row, string key)
        {
            if (!row.TryGetValue(key, out var value))
            {
                throw new InvalidOperationException($"Missing '{key}' key in create customer duplicate companies data table mapping.");
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

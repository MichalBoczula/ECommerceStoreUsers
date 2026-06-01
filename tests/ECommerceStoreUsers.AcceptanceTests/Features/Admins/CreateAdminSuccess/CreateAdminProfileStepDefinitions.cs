using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins.CreateAdminSuccess
{
    [Binding]
    public class CreateAdminProfileStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateAdminRequestDto? _request;

        public CreateAdminProfileStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have a valid create admin request")]
        public void GivenIHaveAValidCreateAdminRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Create admin request table", requestValues, _apiContext.JsonOptions);

            _request = new CreateAdminRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExternalId"),
                FullName = GetRequiredValue(requestValues, "FullName"),
                Email = GetRequiredValue(requestValues, "Email")
            };

            AllureJson.AttachObject(
                "Create admin request",
                _request,
                _apiContext.JsonOptions);
        }

        [When("I submit the create admin request")]
        public async Task WhenISubmitTheCreateAdminRequest()
        {
            _request.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/admins",
                _request,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the admin profile is created successfully")]
        public async Task ThenTheAdminProfileIsCreatedSuccessfully(Table table)
        {
            var expected = ParseExpectedTable(table);

            AllureJson.AttachObject("Expected result table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var adminResponse = await DeserializeResponse<AdminResponseDto>(_apiContext.Response);
            adminResponse.ShouldNotBeNull();

            if (TryGetBool(expected, "HasId", out var hasId))
            {
                if (hasId)
                {
                    adminResponse!.Id.ShouldNotBe(Guid.Empty);
                }
                else
                {
                    adminResponse!.Id.ShouldBe(Guid.Empty);
                }
            }

            adminResponse!.ExternalId.ShouldBe(GetExpectedValue(expected, "ExternalId", adminResponse.ExternalId));
            adminResponse.FullName.ShouldBe(GetExpectedValue(expected, "FullName", adminResponse.FullName));
            adminResponse.Email.ShouldBe(GetExpectedValue(expected, "Email", adminResponse.Email));
        }

        private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _apiContext.JsonOptions);
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
                throw new InvalidOperationException($"Missing '{key}' value in admin expected result table.");
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

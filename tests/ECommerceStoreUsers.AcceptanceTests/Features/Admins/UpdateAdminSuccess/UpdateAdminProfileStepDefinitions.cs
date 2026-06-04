using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins.UpdateAdminSuccess
{
    [Binding]
    public class UpdateAdminProfileStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateAdminRequestDto? _createdAdminRequest;
        private UpdateAdminProfileRequestDto? _updateRequest;
        private Guid _adminId;

        public UpdateAdminProfileStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("an admin profile exists for update admin request")]
        public async Task GivenAnAdminProfileExistsForUpdateAdminRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update admin setup request table", requestValues, _apiContext.JsonOptions);

            _createdAdminRequest = new CreateAdminRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExternalId"),
                FullName = GetRequiredValue(requestValues, "FullName"),
                Email = GetRequiredValue(requestValues, "Email")
            };

            AllureJson.AttachObject(
                "Update admin setup create request",
                _createdAdminRequest,
                _apiContext.JsonOptions);

            var response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/admins",
                _createdAdminRequest,
                _apiContext.JsonOptions);

            var body = await response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Update admin setup response JSON ({(int)response.StatusCode})", body);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var adminResponse = await DeserializeResponse<AdminResponseDto>(response);
            adminResponse.ShouldNotBeNull();
            adminResponse!.Id.ShouldNotBe(Guid.Empty);

            _adminId = adminResponse.Id;
        }

        [Given("I have a valid update admin request")]
        public void GivenIHaveAValidUpdateAdminRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update admin request table", requestValues, _apiContext.JsonOptions);

            _updateRequest = new UpdateAdminProfileRequestDto
            {
                FullName = GetRequiredValue(requestValues, "FullName"),
                Email = GetRequiredValue(requestValues, "Email")
            };

            AllureJson.AttachObject(
                "Update admin request",
                _updateRequest,
                _apiContext.JsonOptions);
        }

        [When("I submit the valid update admin request")]
        public async Task WhenISubmitTheValidUpdateAdminRequest()
        {
            _adminId.ShouldNotBe(Guid.Empty);
            _updateRequest.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PutAsJsonAsync(
                $"/admins/{_adminId}",
                _updateRequest,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Update admin response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the admin profile is updated successfully")]
        public async Task ThenTheAdminProfileIsUpdatedSuccessfully(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected update admin result table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var adminResponse = await DeserializeResponse<AdminResponseDto>(_apiContext.Response);
            adminResponse.ShouldNotBeNull();

            if (TryGetBool(expected, "HasId", out var hasId))
            {
                if (hasId)
                {
                    adminResponse!.Id.ShouldNotBe(Guid.Empty);
                    adminResponse.Id.ShouldBe(_adminId);
                }
                else
                {
                    adminResponse!.Id.ShouldBe(Guid.Empty);
                }
            }

            adminResponse!.ExternalId.ShouldBe(GetExpectedValue(expected, "ExternalId", adminResponse.ExternalId));
            adminResponse.FullName.ShouldBe(GetExpectedValue(expected, "FullName", adminResponse.FullName));
            adminResponse.Email.ShouldBe(GetExpectedValue(expected, "Email", adminResponse.Email));

            if (TryGetBool(expected, "IsActive", out var isActive))
            {
                adminResponse.IsActive.ShouldBe(isActive);
            }

            if (TryGetBool(expected, "HasLastLoginAt", out var hasLastLoginAt))
            {
                if (hasLastLoginAt)
                {
                    adminResponse.LastLoginAt.ShouldNotBe(default);
                }
                else
                {
                    adminResponse.LastLoginAt.ShouldBe(default);
                }
            }
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
                throw new InvalidOperationException($"Missing '{key}' value in update admin data table.");
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

using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins.UpdateAdminValidationError
{
    [Binding]
    public class UpdateAdminProfileValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private UpdateAdminProfileRequestDto? _request;
        private Guid _adminId;

        public UpdateAdminProfileValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have an existing admin profile and an invalid update admin request")]
        public async Task GivenIHaveAnExistingAdminProfileAndAnInvalidUpdateAdminRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update admin validation setup table", requestValues, _apiContext.JsonOptions);

            var createRequest = new CreateAdminRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExistingExternalId"),
                FullName = GetRequiredValue(requestValues, "ExistingFullName"),
                Email = GetRequiredValue(requestValues, "ExistingEmail")
            };

            AllureJson.AttachObject(
                "Existing admin create payload",
                createRequest,
                _apiContext.JsonOptions);

            var createResponse = await _apiContext.HttpClient.PostAsJsonAsync(
                "/admins",
                createRequest,
                _apiContext.JsonOptions);

            var createBody = await createResponse.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Existing admin creation response JSON ({(int)createResponse.StatusCode})", createBody);

            createResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var createdAdmin = JsonSerializer.Deserialize<AdminResponseDto>(createBody, _apiContext.JsonOptions);
            createdAdmin.ShouldNotBeNull();
            createdAdmin!.Id.ShouldNotBe(Guid.Empty);
            _adminId = createdAdmin.Id;

            _request = new UpdateAdminProfileRequestDto
            {
                FullName = GetRequiredValue(requestValues, "UpdatedFullName"),
                Email = GetRequiredValue(requestValues, "UpdatedEmail")
            };

            AllureJson.AttachObject(
                "Update admin invalid request payload",
                _request,
                _apiContext.JsonOptions);
        }

        [When("I submit the update admin request for validation")]
        public async Task WhenISubmitTheUpdateAdminRequestForValidation()
        {
            _request.ShouldNotBeNull();
            _adminId.ShouldNotBe(Guid.Empty);

            _apiContext.Response = await _apiContext.HttpClient.PutAsJsonAsync(
                $"/admins/{_adminId}",
                _request,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Validation Response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the admin profile update fails with a validation error")]
        public async Task ThenTheAdminProfileUpdateFailsWithAValidationError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected update validation result table", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<ApiProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();
            problemDetails.Errors.ShouldNotBeEmpty();

            var expectedMessage = GetRequiredValue(expected, "Message");
            var expectedName = GetRequiredValue(expected, "Name");
            var expectedEntity = GetRequiredValue(expected, "Entity");

            var emailError = problemDetails.Errors.FirstOrDefault(e => e.Name == expectedName);

            emailError.ShouldNotBeNull($"Expected validation rule '{expectedName}' was not triggered.");
            emailError.Message.ShouldBe(expectedMessage);
            emailError.Entity.ShouldBe(expectedEntity);

            problemDetails.TraceId.ShouldNotBeNullOrWhiteSpace();
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

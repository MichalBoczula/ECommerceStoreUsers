using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins.CreateAdminValidationError
{
    [Binding]
    public class CreateAdminProfileValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateAdminRequestDto? _request;

        public CreateAdminProfileValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have an invalid create admin request")]
        public void GivenIHaveAnInvalidCreateAdminRequest(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Create admin invalid request table", requestValues, _apiContext.JsonOptions);

            _request = new CreateAdminRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExternalId"),
                FullName = GetRequiredValue(requestValues, "FullName"),
                Email = GetRequiredValue(requestValues, "Email")
            };

            AllureJson.AttachObject(
                "Create admin invalid request payload",
                _request,
                _apiContext.JsonOptions);
        }

        [When("I submit the create admin request for validation")]
        public async Task WhenISubmitTheCreateAdminRequestForValidation()
        {
            _request.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/admins",
                _request,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Validation Response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the admin profile creation fails with a validation error")]
        public async Task ThenTheAdminProfileCreationFailsWithAValidationError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected validation result table", expected, _apiContext.JsonOptions);

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
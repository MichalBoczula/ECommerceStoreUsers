using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins.CreateAdminConflictError
{
    [Binding]
    public class CreateAdminProfileConflictStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private CreateAdminRequestDto? _request;

        public CreateAdminProfileConflictStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have a valid create admin request payload")]
        public void GivenIHaveAValidCreateAdminRequestPayload(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Conflict request data table", requestValues, _apiContext.JsonOptions);

            _request = new CreateAdminRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExternalId"),
                FullName = GetRequiredValue(requestValues, "FullName"),
                Email = GetRequiredValue(requestValues, "Email")
            };
        }

        [Given("an admin profile with the same external id already exists")]
        public async Task GivenAnAdminProfileWithTheSameExternalIdAlreadyExists()
        {
            _request.ShouldNotBeNull();

            var initialResponse = await _apiContext.HttpClient.PostAsJsonAsync(
                "/admins",
                _request,
                _apiContext.JsonOptions);

            initialResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            AllureJson.AttachObject("Initial setup admin created successfully", _request, _apiContext.JsonOptions);
        }

        [When("I submit the duplicate create admin request")]
        public async Task WhenISubmitTheDuplicateCreateAdminRequest()
        {
            _request.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PostAsJsonAsync(
                "/admins",
                _request,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Conflict Response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the admin profile creation fails with a conflict error")]
        public void ThenTheAdminProfileCreationFailsWithAConflictError(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected conflict table results", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();

            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));
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
                throw new InvalidOperationException($"Missing '{key}' key in data table mapping.");
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
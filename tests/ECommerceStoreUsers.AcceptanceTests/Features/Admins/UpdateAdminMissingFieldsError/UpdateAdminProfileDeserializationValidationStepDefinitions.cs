using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using Microsoft.AspNetCore.Mvc;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins.UpdateAdminMissingFieldsError
{
    [Binding]
    public class UpdateAdminProfileDeserializationValidationStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private Dictionary<string, string>? _rawPayload;
        private Guid _adminId;

        public UpdateAdminProfileDeserializationValidationStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I have an existing admin profile and an update admin request payload missing the full name")]
        public async Task GivenIHaveAnExistingAdminProfileAndAnUpdateAdminRequestPayloadMissingTheFullName(Table table)
        {
            var requestValues = ParseExpectedTable(table);
            AllureJson.AttachObject("Update admin missing fields setup table", requestValues, _apiContext.JsonOptions);

            var createRequest = new CreateAdminRequestDto
            {
                ExternalId = GetRequiredValue(requestValues, "ExistingExternalId"),
                FullName = GetRequiredValue(requestValues, "ExistingFullName"),
                Email = GetRequiredValue(requestValues, "ExistingEmail")
            };

            AllureJson.AttachObject(
                "Existing admin create payload for missing fields flow",
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

            _rawPayload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Email"] = GetRequiredValue(requestValues, "Email")
            };

            AllureJson.AttachObject(
                "Incomplete update admin raw request JSON dictionary structure",
                _rawPayload,
                _apiContext.JsonOptions);
        }

        [When("I submit the incomplete update admin request")]
        public async Task WhenISubmitTheIncompleteUpdateAdminRequest()
        {
            _adminId.ShouldNotBe(Guid.Empty);
            _rawPayload.ShouldNotBeNull();

            _apiContext.Response = await _apiContext.HttpClient.PutAsJsonAsync(
                $"/admins/{_adminId}",
                _rawPayload,
                _apiContext.JsonOptions);

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Update admin deserialization exception response JSON ({(int)_apiContext.Response.StatusCode})", body);
        }

        [Then("the update admin response indicates a json deserialization failure")]
        public async Task ThenTheUpdateAdminResponseIndicatesAJsonDeserializationFailure(Table table)
        {
            var expected = ParseExpectedTable(table);
            AllureJson.AttachObject("Expected update admin deserialization error constraints", expected, _apiContext.JsonOptions);

            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response!.StatusCode.ShouldBe(ParseStatusCode(expected, "StatusCode"));

            var problemDetails = await DeserializeResponse<ProblemDetails>(_apiContext.Response);
            problemDetails.ShouldNotBeNull();
            problemDetails!.Status.ShouldBe((int)ParseStatusCode(expected, "StatusCode"));
            problemDetails.Title.ShouldBe(GetRequiredValue(expected, "Title"));
            problemDetails.Detail.ShouldBe(GetRequiredValue(expected, "Detail"));
            problemDetails.Type.ShouldBe(GetRequiredValue(expected, "Type"));
            problemDetails.Instance.ShouldBe($"/admins/{_adminId}");
            problemDetails.Extensions.TryGetValue("traceId", out var traceId).ShouldBeTrue();
            traceId.ShouldNotBeNull();
            problemDetails.Detail.ShouldContain(GetRequiredValue(expected, "MissingProperty"));
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
                throw new InvalidOperationException($"Missing '{key}' value in update admin missing fields data table.");
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

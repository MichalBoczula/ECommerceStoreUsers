using Reqnroll;
using Shouldly;
using System.Net;
using System.Globalization;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common
{
    [Binding]
    public sealed class HealthEndpointStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;

        public HealthEndpointStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [When("I request the service health endpoint")]
        public async Task WhenIRequestTheServiceHealthEndpoint()
        {
            _apiContext.Response = await _apiContext.HttpClient.GetAsync("/health");
        }

        [When("I request the ready health endpoint")]
        public async Task WhenIRequestTheReadyHealthEndpoint()
        {
            _apiContext.Response = await _apiContext.HttpClient.GetAsync("/health/ready");
        }

        [When("I request health route {string}")]
        public async Task WhenHealthRoute(string path)
        {
            _apiContext.Response = await _apiContext.HttpClient.GetAsync(path);
        }

        [Then("the health response contains these exact values")]
        public async Task ThenExactHealth(Table table)
        {
            var expected = table.Rows.ToDictionary(row => row["Field"], row => row["Value"]);
            _apiContext.RecordEvidence("Technical health expectations", expected);
            _apiContext.Response.ShouldNotBeNull();
            var response = _apiContext.Response;
            ((int)response.StatusCode).ShouldBe(int.Parse(expected["Status"], CultureInfo.InvariantCulture));
            response.RequestMessage!.RequestUri!.AbsolutePath.ShouldBe(expected["Path"]);
            response.Content.Headers.ContentType?.MediaType.ShouldBe(expected["ContentType"]);
            (await response.Content.ReadAsStringAsync()).ShouldBe(expected["Body"]);
            response.Headers.CacheControl.ShouldNotBeNull();
            response.Headers.CacheControl.NoStore.ShouldBeTrue();
            response.Headers.CacheControl.NoCache.ShouldBeTrue();
            response.Headers.Pragma.Select(value => value.Name).ShouldContain("no-cache");
            response.Content.Headers.Expires.ShouldBe(DateTimeOffset.UnixEpoch);
            _apiContext.RecordEvidence("Technical health verified", new
            {
                status = (int)response.StatusCode,
                body = await response.Content.ReadAsStringAsync(),
                cacheControl = response.Headers.CacheControl.ToString(),
                pragma = response.Headers.Pragma.ToString(),
                expires = response.Content.Headers.Expires,
                endpointKind = "Plain-text health middleware; not an API problem response"
            });
        }

        [Then("the health response status is 200")]
        public void ThenTheHealthResponseStatusIs200()
        {
            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}


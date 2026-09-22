using Reqnroll;
using Shouldly;
using System.Net;

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

        [Then("the health response status is 200")]
        public void ThenTheHealthResponseStatusIs200()
        {
            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}

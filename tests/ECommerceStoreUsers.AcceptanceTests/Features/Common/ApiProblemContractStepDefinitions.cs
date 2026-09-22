using System.Net;
using System.Text;
using System.Text.Json;
using Reqnroll;
using Shouldly;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class ApiProblemContractStepDefinitions(ScenarioApiContext apiContext)
{
    private string? _path;
    private string? _errorCase;

    [When("I trigger the Users REF-06 error case {string}")]
    public async Task WhenITriggerTheErrorCase(string errorCase)
    {
        var client = apiContext.HttpClient;
        _errorCase = errorCase;
        (_path, apiContext.Response) = errorCase switch
        {
            "route" => ("/ref-06-not-found", await client.GetAsync("/ref-06-not-found")),
            "method" => ("/customers/external/test", await client.PostAsync(
                "/customers/external/test", null)),
            "media" => ("/customers", await client.PostAsync(
                "/customers", new StringContent("{}", Encoding.UTF8, "text/plain"))),
            "json" => ("/customers", await client.PostAsync(
                "/customers", new StringContent("{INTERNAL_FAILURE_MARKER", Encoding.UTF8, "application/json"))),
            "missing" => ("/favorites/clients/11111111-1111-1111-1111-111111111111", await client.PostAsync(
                "/favorites/clients/11111111-1111-1111-1111-111111111111",
                new StringContent("{}", Encoding.UTF8, "application/json"))),
            "body" => ("/customers", await client.PostAsync(
                "/customers", new StringContent("", Encoding.UTF8, "application/json"))),
            _ => throw new ArgumentOutOfRangeException(nameof(errorCase))
        };
    }

    [Then("the Users REF-06 response has status {int} and code {string}")]
    public async Task ThenTheResponseHasProblemContract(int status, string code)
    {
        apiContext.Response.ShouldNotBeNull();
        apiContext.Response.StatusCode.ShouldBe((HttpStatusCode)status);
        apiContext.Response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await apiContext.Response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var problem = document.RootElement;
        problem.GetProperty("status").GetInt32().ShouldBe(status);
        problem.GetProperty("code").GetString().ShouldBe(code);
        problem.GetProperty("type").GetString().ShouldNotBeNullOrWhiteSpace();
        problem.GetProperty("title").GetString().ShouldNotBeNullOrWhiteSpace();
        problem.GetProperty("detail").GetString().ShouldNotBeNullOrWhiteSpace();
        problem.GetProperty("instance").GetString().ShouldBe(_path);
        problem.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
        problem.GetProperty("errors").GetArrayLength().ShouldBe(0);
        var missing = problem.GetProperty("missingProperties");
        if (_errorCase == "missing")
        {
            missing.EnumerateArray().Select(item => item.GetString()).ShouldContain("productId");
        }
        else
        {
            missing.GetArrayLength().ShouldBe(0);
        }
        body.ShouldNotContain("INTERNAL_FAILURE_MARKER");
        body.ShouldNotContain("stackTrace");
    }
}

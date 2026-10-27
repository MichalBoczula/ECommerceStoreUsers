using Reqnroll;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class ApiProblemContractStepDefinitions(FrameworkFailureState state)
{
    private string? _errorCase;

    [When("I trigger the Users REF-06 error case {string}")]
    public async Task WhenITriggerTheErrorCase(string errorCase)
    {
        _errorCase = errorCase;
        var (method, path, body, contentType) = errorCase switch
        {
            "route" => ("GET", "/ref-06-not-found", "<no-content>", "<none>"),
            "method" => ("POST", "/customers/external/test", "<no-content>", "<none>"),
            "media" => ("POST", "/customers", "{}", "text/plain; charset=utf-8"),
            "json" => ("POST", "/customers", "{INTERNAL_FAILURE_MARKER", "application/json; charset=utf-8"),
            "type" => ("POST", "/favorites/clients/11111111-1111-1111-1111-111111111111", "{\"productId\":\"not-a-guid\"}", "application/json; charset=utf-8"),
            "null" => ("POST", "/favorites/clients/11111111-1111-1111-1111-111111111111", "{\"productId\":null}", "application/json; charset=utf-8"),
            "missing" => ("POST", "/favorites/clients/11111111-1111-1111-1111-111111111111", "{}", "application/json; charset=utf-8"),
            "body" => ("POST", "/customers", "<empty>", "application/json; charset=utf-8"),
            _ => throw new ArgumentOutOfRangeException(nameof(errorCase))
        };
        state.Prepare(method, path, body, contentType);
        await state.SendAsync();
    }

    [Then("the Users REF-06 response has status {int} and code {string}")]
    public async Task ThenTheResponseHasProblemContract(int status, string code)
    {
        var (title, detail, missing, allow) = _errorCase switch
        {
            "route" => ("Route not found.", "The requested route does not exist.", "[]", "[]"),
            "method" => ("Method not allowed.", "The requested method is not supported by this route.", "[]", "GET"),
            "media" => ("Unsupported media type.", "The request content type is not supported.", "[]", "[]"),
            "body" => ("Invalid request.", "The request could not be processed.", "[]", "[]"),
            "missing" => ("Invalid JSON payload.", "JSON payload for AddFavoriteRequestDto is missing required properties: productId.", "productId", "[]"),
            "json" or "type" or "null" => ("Invalid JSON payload.", "The request body is not valid JSON.", "[]", "[]"),
            _ => throw new InvalidOperationException("No framework request was sent.")
        };
        await state.ExpectProblemAsync(status, code, title, detail, missing, allow);
        await state.ExpectUnchangedAsync();
    }
}

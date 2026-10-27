using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Configuration;

public sealed class OpenApiRoutingTests
{
    [Theory]
    [InlineData("not-a-guid", false)]
    [InlineData("11111111-1111-1111-1111-111111111111", true)]
    public async Task GuidConstraintRejectionIsFramework404ButUndeclaredMatched404StillFails(string clientId, bool rejects)
    {
        using var document = JsonDocument.Parse("""
            {"paths":{"/favorites/clients/{clientId}":{"get":{
              "operationId":"GetFavoritesByClientId",
              "parameters":[{"name":"clientId","in":"path","schema":{"type":"string","format":"uuid"}}],
              "responses":{"200":{"description":"OK"}}
            }}},"components":{"schemas":{}}}
            """);
        using var client = new HttpClient(new OpenApiResponseHandler(document) { InnerHandler = new NotFoundHandler() });
        var request = $"http://localhost/favorites/clients/{clientId}";
        if (rejects)
        {
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetAsync(request));
            Assert.Contains("Undeclared runtime status: GetFavoritesByClientId 404", error.Message);
        }
        else
        {
            using var response = await client.GetAsync(request);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    private sealed class NotFoundHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("""
                    {"status":404,"code":"route_not_found","type":"about:blank","title":"Route not found."}
                    """, Encoding.UTF8, "application/problem+json")
            });
    }
}

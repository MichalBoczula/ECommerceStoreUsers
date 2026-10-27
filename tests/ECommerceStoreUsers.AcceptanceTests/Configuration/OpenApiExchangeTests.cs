using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Configuration;

public sealed class OpenApiExchangeTests
{
    [Theory]
    [InlineData(200, true)]
    [InlineData(400, false)]
    public async Task InvalidInputReachesTransportAndOnlySuccessRequiresRequestConformance(int status, bool fails)
    {
        using var document = JsonDocument.Parse("""
            {"paths":{"/input":{"post":{"operationId":"Input",
              "requestBody":{"content":{"application/json":{"schema":{"type":"string","format":"uuid"}}}},
              "responses":{
                "200":{"content":{"application/json":{"schema":{"type":"boolean"}}}},
                "400":{"content":{"application/problem+json":{"schema":{"type":"boolean"}}}}
              }
            }}},"components":{"schemas":{}}}
            """);
        var observations = new List<JsonElement>();
        var transport = new Transport(status);
        using var client = new HttpClient(new OpenApiResponseHandler(document, (_, value) => observations.Add(JsonSerializer.SerializeToElement(value))) { InnerHandler = transport });
        using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/input") { Content = new StringContent("\"not-a-uuid\"", Encoding.UTF8, "application/json") };
        if (fails)
            await Assert.ThrowsAsync<InvalidOperationException>(() => client.SendAsync(request));
        else
        {
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("true", await response.Content.ReadAsStringAsync());
        }
        Assert.Equal(1, transport.Calls);
        Assert.Equal("\"not-a-uuid\"", transport.Body);
        var observed = Assert.Single(observations);
        Assert.False(observed.GetProperty("request").GetProperty("conforms").GetBoolean());
        Assert.True(observed.GetProperty("response").GetProperty("conforms").GetBoolean());
        Assert.Equal(status, observed.GetProperty("status").GetInt32());
    }

    private sealed class Transport(int status) : HttpMessageHandler
    {
        internal int Calls { get; private set; }
        internal string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage((HttpStatusCode)status)
            {
                Content = new StringContent("true", Encoding.UTF8, status >= 400 ? "application/problem+json" : "application/json")
            };
        }
    }
}

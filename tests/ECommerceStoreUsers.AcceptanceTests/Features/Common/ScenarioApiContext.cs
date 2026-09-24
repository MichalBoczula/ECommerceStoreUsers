using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common
{
    public sealed class ScenarioApiContext
    {
        public string DatabaseName { get; set; } = string.Empty;
        public ApplicationFactory Factory { get; set; } = default!;
        public HttpClient HttpClient { get; set; } = default!;
        public JsonDocument OpenApiDocument { get; set; } = default!;
        public HttpResponseMessage? Response { get; set; }
        public Guid FavoriteClientId { get; set; }
        public Guid FavoriteProductId { get; set; }

        public JsonSerializerOptions JsonOptions { get; } = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }
}

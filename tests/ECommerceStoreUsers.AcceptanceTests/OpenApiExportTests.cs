using ECommerceStoreUsers.API;
using ECommerceStoreUsers.API.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests;

public sealed class OpenApiExportTests
{
    [Fact]
    public async Task ExportGeneratedOpenApiWithoutMongo()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["MongoDbSettings:ConnectionString"] = "mongodb://127.0.0.1:27017"
                }));
            builder.ConfigureTestServices(services =>
            {
                var initializer = services.Single(service =>
                    service.ServiceType == typeof(IHostedService) &&
                    service.ImplementationType == typeof(MongoInitializationHostedService));
                services.Remove(initializer);
            });
        });

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();

        var openApi = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(openApi);
        Assert.True(document.RootElement.GetProperty("paths").EnumerateObject().Any());

        var exportPath = Environment.GetEnvironmentVariable("OPENAPI_EXPORT_PATH");
        if (!string.IsNullOrWhiteSpace(exportPath))
        {
            await File.WriteAllTextAsync(exportPath, openApi);
        }
    }
}

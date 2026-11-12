using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Application.Services.Abstract.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Infrastructure.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PactNet.Verifier;

namespace ECommerceStoreUsers.AcceptanceTests.Configuration;

public sealed class UsersPactProviderTests
{
    private const string StatePath = "/pact-provider-state";

    [Fact]
    public Task VerifyPinnedWebPactAgainstUsersApi() => VerifyPinnedPactAsync("web", "USERS_PACT_REPORT");

    [Fact]
    public Task VerifyPinnedBffPactAgainstUsersApi() => VerifyPinnedPactAsync("bff", "USERS_BFF_PACT_REPORT");

    private static async Task VerifyPinnedPactAsync(string consumer, string reportVariable)
    {
        Assert.True(Environment.GetEnvironmentVariable("PACT_DO_NOT_TRACK") == "true",
            "Start the Pact test host with PACT_DO_NOT_TRACK=true.");
        var directory = Path.Combine(AppContext.BaseDirectory, "pact");
        var pactPath = Path.Combine(directory, $"{consumer}-users.json");
        var pactBytes = await File.ReadAllBytesAsync(pactPath);
        using var source = JsonDocument.Parse(await File.ReadAllBytesAsync(Path.Combine(directory, $"{consumer}-source.json")));
        using var pact = JsonDocument.Parse(pactBytes);
        var hash = Convert.ToHexStringLower(SHA256.HashData(pactBytes));
        var interactions = pact.RootElement.GetProperty("interactions").GetArrayLength();

        Assert.Equal(source.RootElement.GetProperty("contractSha256").GetString(), hash);
        Assert.Equal(source.RootElement.GetProperty("interactionCount").GetInt32(), interactions);
        Assert.True(interactions > 0);
        Assert.Equal(consumer, pact.RootElement.GetProperty("consumer").GetProperty("name").GetString());
        Assert.Equal("users", pact.RootElement.GetProperty("provider").GetProperty("name").GetString());
        Assert.Equal("4.0", pact.RootElement.GetProperty("metadata").GetProperty("pactSpecification").GetProperty("version").GetString());

        var (startedAt, finishedAt) = await VerifyAsync(pactPath);

        var report = Environment.GetEnvironmentVariable(reportVariable);
        if (!string.IsNullOrWhiteSpace(report))
        {
            await File.WriteAllTextAsync(report, JsonSerializer.Serialize(new
            {
                consumerCommitSha = source.RootElement.GetProperty("commitSha").GetString(),
                providerCommitSha = Environment.GetEnvironmentVariable("GITHUB_SHA"),
                contractSha256 = hash,
                verifierName = "PactNet",
                verifierVersion = "5.0.1",
                status = "Passed",
                verifiedInteractions = interactions,
                failedInteractions = 0,
                summary = "Users HTTP provider Pact verification passed.",
                issues = Array.Empty<string>(),
                startedAt,
                finishedAt
            }));
        }
    }

    private static async Task<(DateTimeOffset Started, DateTimeOffset Finished)> VerifyAsync(string pactPath)
    {
        var databaseName = $"acceptance-{Guid.NewGuid():N}";
        try
        {
            await using var factory = new ApplicationFactory(AcceptanceMongoDb.ConnectionString, databaseName);
            await using var host = factory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => services.AddTransient<IStartupFilter, PactStates>()));
            host.UseKestrel(options => options.Listen(IPAddress.Loopback, 0));

            using var client = host.CreateClient();
            var endpoint = client.BaseAddress ?? throw new InvalidOperationException("Expected a Users HTTP listener.");
            using var verifier = new PactVerifier("users");
            var started = DateTimeOffset.UtcNow;
            verifier.WithHttpEndpoint(endpoint)
                .WithFileSource(new FileInfo(pactPath))
                .WithProviderStateUrl(new Uri(endpoint, StatePath))
                .Verify();
            return (started, DateTimeOffset.UtcNow);
        }
        finally
        {
            await AcceptanceMongoDb.DropDatabaseAsync(databaseName);
        }
    }

    private sealed class PactStates : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Map(StatePath, state => state.Run(SetupAsync));
            next(app);
        };

        private static async Task SetupAsync(HttpContext context)
        {
            var request = await context.Request.ReadFromJsonAsync<JsonElement>(context.RequestAborted);
            var parameters = request.GetProperty("params");
            var services = context.RequestServices;
            var cancellationToken = context.RequestAborted;

            switch (request.GetProperty("state").GetString())
            {
                case "one customer profile exists and another external ID is absent":
                    await SetupProfileLookupAsync(parameters, services, cancellationToken);
                    break;
                case "the BFF registration customer exists":
                    await SetupRegistrationAsync(parameters, services, exists: true, cancellationToken);
                    break;
                case "the BFF registration customer is absent":
                    await SetupRegistrationAsync(parameters, services, exists: false, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported Users Pact provider state.");
            }

            await context.Response.WriteAsJsonAsync(new { }, cancellationToken);
        }

        private static async Task SetupProfileLookupAsync(
            JsonElement parameters, IServiceProvider services, CancellationToken cancellationToken)
        {
            var profile = parameters.GetProperty("profile").Deserialize<CustomerResponseDto>(JsonSerializerOptions.Web)
                ?? throw new InvalidOperationException("Expected a Users Pact customer profile.");
            var missingExternalId = parameters.GetProperty("missingExternalId").GetString()
                ?? throw new InvalidOperationException("Expected the missing Users Pact external ID.");
            var repository = services.GetRequiredService<ICustomerRepository>();

            if (profile.ExternalId == missingExternalId ||
                await repository.GetByExternalIdAsync(missingExternalId, cancellationToken) is not null)
                throw new InvalidOperationException("The Pact's missing profile unexpectedly exists.");

            if (await repository.GetByExternalIdAsync(profile.ExternalId, cancellationToken) is null)
            {
                var individual = profile.Individual;
                var customer = Customer.Rehydrate(
                    profile.Id, profile.ExternalId,
                    new IndividualData(individual.FirstName, individual.LastName, individual.Email, individual.Phone,
                        ToAddress(individual.BillingAddress), ToAddress(individual.ShippingAddress)),
                    profile.Companies.Select(company => CompanyData.Rehydrate(
                        company.Id, company.TaxId, company.CompanyName,
                        ToAddress(company.BillingAddress), ToAddress(company.ShippingAddress))).ToList(),
                    profile.UpdatedAt);
                await repository.CreateCustomer(customer, cancellationToken);
            }
        }

        private static async Task SetupRegistrationAsync(
            JsonElement parameters, IServiceProvider services, bool exists, CancellationToken cancellationToken)
        {
            var registration = parameters.GetProperty("registrationJson")
                .Deserialize<CreateCustomerRequestDto>(JsonSerializerOptions.Web)
                ?? throw new InvalidOperationException("Expected a BFF Pact registration.");
            var repository = services.GetRequiredService<ICustomerRepository>();
            var customer = await repository.GetByExternalIdAsync(registration.ExternalId, cancellationToken);

            if (exists && customer is null)
            {
                await services.GetRequiredService<ICustomerService>().CreateCustomer(registration, cancellationToken);
            }
            else if (!exists && customer is not null)
            {
                // The create interaction can precede the missing lookup in this isolated database.
                var database = services.GetRequiredService<MongoDbContext>();
                await database.Customers.DeleteOneAsync(item => item.Id == customer.Id, cancellationToken);
                await database.CustomersHistory.DeleteManyAsync(item => item.CustomerId == customer.Id, cancellationToken);
            }
        }

        private static Address ToAddress(AddressResponseDto dto) =>
            new(dto.PostalCode, dto.City, dto.Street, dto.BuildingNumber, dto.ApartmentNumber);
    }
}

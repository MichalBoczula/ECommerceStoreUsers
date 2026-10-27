using Xunit.Abstractions;
using ECommerceStoreUsers.Infrastructure.Context;
using Microsoft.AspNetCore.TestHost;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using ECommerceStoreUsers.API.Configuration;
using ECommerceStoreUsers.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using System.Net;
using Testcontainers.MongoDb;

namespace ECommerceStoreUsers.AcceptanceTests.Configuration;

public sealed class MongoStartupLifecycleTests(ITestOutputHelper output)
{
    [Fact]
    public async Task StartupWaitsForTemporarilyUnavailableReplicaSetAndThenServesRequests()
    {
        await using var container = new MongoDbBuilder("mongo:8.0").WithReplicaSet().Build();
        await container.StartAsync();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var original = new ApplicationFactory(container.GetConnectionString(), $"acceptance-{Guid.NewGuid():N}");
        using var factory = original.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            var registration = services.Single(service => service.ServiceType == typeof(IHostedService) && service.ImplementationType == typeof(MongoInitializationHostedService));
            services.Remove(registration);
            services.AddSingleton<MongoInitializationHostedService>();
            services.AddSingleton<IHostedService>(provider => new ObservedStartup(provider.GetRequiredService<MongoInitializationHostedService>(), entered, output));
        }));

        await container.PauseAsync();
        var paused = true;
        try
        {
            var starting = Task.Run(() => factory.CreateClient());
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(8));
            starting.IsCompleted.ShouldBeFalse();
            output.WriteLine("Startup is pending inside the real Mongo initializer while the replica set is paused.");
            await container.UnpauseAsync();
            paused = false;

            using var client = await starting.WaitAsync(TimeSpan.FromSeconds(18));
            await MongoReadinessTests.ExpectHealthAsync(client, "/health/ready", HttpStatusCode.OK, output);
            var mongo = factory.Services.GetRequiredService<MongoDbContext>();
            foreach (var (name, expected) in new Dictionary<string, string[]>
            {
                ["customers"] = ["_id_", "UX_Customer_ExternalId", "IX_Customer_Companies_TaxId"],
                ["customers-history"] = ["_id_", "IX_CustomersHistory_CustomerId"],
                ["admins"] = ["_id_", "UX_Admin_ExternalId"],
                ["admins-history"] = ["_id_", "IX_AdminHistory_AdminId"],
                ["favorites"] = ["_id_", "UX_Favorite_ClientId_ProductId"]
            })
            {
                var collection = mongo.Customers.Database.GetCollection<BsonDocument>(name);
                var indexes = await (await collection.Indexes.ListAsync()).ToListAsync();
                indexes.Select(index => index["name"].AsString).Order().ShouldBe(expected.Order());
                foreach (var index in indexes.Where(index => index["name"].AsString.StartsWith("UX_", StringComparison.Ordinal)))
                    index["unique"].AsBoolean.ShouldBeTrue();
                (await collection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty)).ShouldBe(0);
                output.WriteLine(JsonSerializer.Serialize(new { phase = "Started", collection = name, indexNames = expected, documentCount = 0, indexesVerified = true }));
            }
        }
        finally
        {
            if (paused)
            {
                await container.UnpauseAsync();
            }
        }
    }

    [Fact]
    public async Task HostStopCancelsAnInProgressMongoStartup()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MongoDbSettings:ConnectionString"] = "mongodb://127.0.0.1:1/?directConnection=true",
            ["MongoDbSettings:DatabaseName"] = "startup-stop",
            ["MongoDbSettings:CustomerCollectionName"] = "customers",
            ["MongoDbSettings:CustomersHistoryCollectionName"] = "customers-history",
            ["MongoDbSettings:AdminCollectionName"] = "admins",
            ["MongoDbSettings:AdminsHistoryCollectionName"] = "admins-history",
            ["MongoDbSettings:FavoriteCollectionName"] = "favorites"
        }).Build();
        await using var services = new ServiceCollection().AddInfrastructure(configuration).BuildServiceProvider();
        using var lifetime = new TestLifetime();
        using var startup = new MongoInitializationHostedService(services, lifetime);
        var starting = startup.StartAsync(CancellationToken.None);

        starting.IsCompleted.ShouldBeFalse();
        output.WriteLine("Real Mongo startup is pending before StopAsync cancellation.");
        await startup.StopAsync(CancellationToken.None);
        await Should.ThrowAsync<OperationCanceledException>(() => starting.WaitAsync(TimeSpan.FromSeconds(5)));
        output.WriteLine("StopAsync cancelled the real startup task.");
    }

    [Theory]
    [InlineData("start-token")]
    [InlineData("host-stopping")]
    public async Task StartupObservesEachLinkedHostCancellation(string source)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MongoDbSettings:ConnectionString"] = "mongodb://127.0.0.1:1/?directConnection=true",
            ["MongoDbSettings:DatabaseName"] = "startup-linked-cancel",
            ["MongoDbSettings:CustomerCollectionName"] = "customers",
            ["MongoDbSettings:CustomersHistoryCollectionName"] = "customers-history",
            ["MongoDbSettings:AdminCollectionName"] = "admins",
            ["MongoDbSettings:AdminsHistoryCollectionName"] = "admins-history",
            ["MongoDbSettings:FavoriteCollectionName"] = "favorites"
        }).Build();
        await using var services = new ServiceCollection().AddInfrastructure(configuration).BuildServiceProvider();
        using var lifetime = new TestLifetime();
        using var startup = new MongoInitializationHostedService(services, lifetime);
        using var cancellation = new CancellationTokenSource();
        var starting = startup.StartAsync(cancellation.Token);
        starting.IsCompleted.ShouldBeFalse();
        if (source == "start-token")
            cancellation.Cancel();
        else
            lifetime.StopApplication();
        await Should.ThrowAsync<OperationCanceledException>(() => starting.WaitAsync(TimeSpan.FromSeconds(5)));
        output.WriteLine(JsonSerializer.Serialize(new { cancellationSource = source, operationCancelled = true, noHttpResponseClaimed = true }));
    }

    private sealed class ObservedStartup(MongoInitializationHostedService inner, TaskCompletionSource entered, ITestOutputHelper output) : IHostedService
    {
        public Task StartAsync(CancellationToken token)
        {
            var starting = inner.StartAsync(token);
            starting.IsCompleted.ShouldBeFalse();
            entered.TrySetResult();
            output.WriteLine("The real Mongo initializer returned an incomplete startup task.");
            return starting;
        }
        public Task StopAsync(CancellationToken token) => inner.StopAsync(token);
    }

    private sealed class TestLifetime : IHostApplicationLifetime, IDisposable
    {
        private readonly CancellationTokenSource _started = new();
        private readonly CancellationTokenSource _stopping = new();
        private readonly CancellationTokenSource _stopped = new();

        public CancellationToken ApplicationStarted => _started.Token;
        public CancellationToken ApplicationStopping => _stopping.Token;
        public CancellationToken ApplicationStopped => _stopped.Token;
        public void StopApplication() => _stopping.Cancel();

        public void Dispose()
        {
            _started.Dispose();
            _stopping.Dispose();
            _stopped.Dispose();
        }
    }
}


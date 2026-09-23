using ECommerceStoreUsers.API.Configuration;
using ECommerceStoreUsers.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using System.Net;
using Testcontainers.MongoDb;

namespace ECommerceStoreUsers.AcceptanceTests.Configuration;

public sealed class MongoStartupLifecycleTests
{
    [Fact]
    public async Task StartupWaitsForTemporarilyUnavailableReplicaSetAndThenServesRequests()
    {
        await using var container = new MongoDbBuilder("mongo:8.0").WithReplicaSet().Build();
        await container.StartAsync();
        using var factory = new ApplicationFactory(container.GetConnectionString(), $"acceptance-{Guid.NewGuid():N}");

        await container.PauseAsync();
        var paused = true;
        try
        {
            var starting = Task.Run(() => factory.CreateClient());
            await Task.Delay(TimeSpan.FromSeconds(4));
            await container.UnpauseAsync();
            paused = false;

            using var client = await starting.WaitAsync(TimeSpan.FromSeconds(18));
            using var ready = await client.GetAsync("/health/ready");
            ready.StatusCode.ShouldBe(HttpStatusCode.OK);
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

        await Task.Delay(200);
        await startup.StopAsync(CancellationToken.None);
        await Should.ThrowAsync<OperationCanceledException>(() => starting);
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Shouldly;
using System.Diagnostics;
using System.Net;
using Testcontainers.MongoDb;

namespace ECommerceStoreUsers.AcceptanceTests.Configuration;

public sealed class MongoReadinessTests
{
    [Fact]
    public async Task PausedReplicaSet_IsUnreadyButLive_AndRecoversAfterUnpause()
    {
        await using var container = new MongoDbBuilder("mongo:8.0").WithReplicaSet().Build();
        await container.StartAsync();

        using var factory = new ApplicationFactory(container.GetConnectionString(), $"acceptance-{Guid.NewGuid():N}");
        using var client = factory.CreateClient();

        var registration = factory.Services.GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value.Registrations.Single(check => check.Name == "mongo-replica-set");
        registration.Timeout.ShouldBe(TimeSpan.FromSeconds(5));
        (await GetStatusAsync(client, "/health/ready")).ShouldBe(HttpStatusCode.OK);

        await container.PauseAsync();
        try
        {
            var elapsed = Stopwatch.StartNew();
            (await GetStatusAsync(client, "/health/ready")).ShouldBe(HttpStatusCode.ServiceUnavailable);
            elapsed.Stop();
            elapsed.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(8));
            (await GetStatusAsync(client, "/health/live")).ShouldBe(HttpStatusCode.OK);
            (await GetStatusAsync(client, "/health")).ShouldBe(HttpStatusCode.OK);
        }
        finally
        {
            await container.UnpauseAsync();
        }

        using var recovery = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        while (true)
        {
            var status = await GetStatusAsync(client, "/health/ready", recovery.Token);
            if (status == HttpStatusCode.OK)
            {
                break;
            }

            await Task.Delay(500, recovery.Token);
        }
    }

    [Fact]
    public async Task StandaloneMongo_IsLiveButUnreadyForTransactions()
    {
        await using var container = new MongoDbBuilder("mongo:8.0").Build();
        await container.StartAsync();

        using var factory = new ApplicationFactory(container.GetConnectionString(), $"acceptance-{Guid.NewGuid():N}");
        using var client = factory.CreateClient();

        (await GetStatusAsync(client, "/health/live")).ShouldBe(HttpStatusCode.OK);
        (await GetStatusAsync(client, "/health/ready")).ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    private static async Task<HttpStatusCode> GetStatusAsync(
        HttpClient client,
        string path,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(path, cancellationToken);
        return response.StatusCode;
    }
}

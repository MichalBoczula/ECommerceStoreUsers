using Xunit.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Shouldly;
using System.Diagnostics;
using System.Net;
using Testcontainers.MongoDb;

namespace ECommerceStoreUsers.AcceptanceTests.Configuration;

public sealed class MongoReadinessTests(ITestOutputHelper output)
{
    [Fact]
    public async Task PreparationWithoutMongo_StartsLiveButRemainsUnready()
    {
        using var factory = new ApplicationFactory(
            "mongodb://127.0.0.1:1/?directConnection=true", "prepare-without-mongo", initializeOnStartup: false);
        using var client = factory.CreateClient();

        await ExpectHealthAsync(client, "/health/live", HttpStatusCode.OK, output);
        await ExpectHealthAsync(client, "/health/ready", HttpStatusCode.ServiceUnavailable, output);
    }

    [Fact]
    public async Task PausedReplicaSet_IsUnreadyButLive_AndRecoversAfterUnpause()
    {
        await using var container = new MongoDbBuilder("mongo:8.0").WithReplicaSet().Build();
        await container.StartAsync();

        using var factory = new ApplicationFactory(container.GetConnectionString(), $"acceptance-{Guid.NewGuid():N}");
        using var client = factory.CreateClient();

        await RecordTopology(container.GetConnectionString(), true);
        var registration = factory.Services.GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value.Registrations.Single(check => check.Name == "mongo-replica-set");
        registration.Timeout.ShouldBe(TimeSpan.FromSeconds(5));
        await ExpectHealthAsync(client, "/health/ready", HttpStatusCode.OK, output);

        await container.PauseAsync();
        try
        {
            var elapsed = Stopwatch.StartNew();
            await ExpectHealthAsync(client, "/health/ready", HttpStatusCode.ServiceUnavailable, output);
            elapsed.Stop();
            elapsed.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(8));
            output.WriteLine(JsonSerializer.Serialize(new { phase = "Paused", readinessTimeoutSeconds = 5, elapsedMilliseconds = elapsed.Elapsed.TotalMilliseconds, result = 503 }));
            await ExpectHealthAsync(client, "/health/live", HttpStatusCode.OK, output);
            await ExpectHealthAsync(client, "/health", HttpStatusCode.OK, output);
        }
        finally
        {
            await container.UnpauseAsync();
        }

        output.WriteLine("MongoDB replica set unpaused; checking readiness recovery.");
        using var recovery = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        while (true)
        {
            var status = await ExpectHealthAsync(client, "/health/ready", null, output, recovery.Token);
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

        await RecordTopology(container.GetConnectionString(), false);
        await ExpectHealthAsync(client, "/health/live", HttpStatusCode.OK, output);
        await ExpectHealthAsync(client, "/health/ready", HttpStatusCode.ServiceUnavailable, output);
    }

    private async Task RecordTopology(string connectionString, bool replicaSet)
    {
        var hello = await new MongoClient(connectionString).GetDatabase("admin").RunCommandAsync<BsonDocument>(new BsonDocument("hello", 1));
        hello.Contains("setName").ShouldBe(replicaSet);
        if (replicaSet)
        {
            hello["isWritablePrimary"].AsBoolean.ShouldBeTrue();
            hello["logicalSessionTimeoutMinutes"].ToDouble().ShouldBeGreaterThan(0);
        }
        output.WriteLine(JsonSerializer.Serialize(new { phase = "Topology", replicaSet, hello = JsonSerializer.Deserialize<JsonElement>(hello.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson })) }));
    }

    internal static async Task<HttpStatusCode> ExpectHealthAsync(HttpClient client, string path,
        HttpStatusCode? expected, ITestOutputHelper output, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(path, cancellationToken);
        if (expected is not null)
            response.StatusCode.ShouldBe(expected.Value);
        ((int)response.StatusCode).ShouldBeOneOf(200, 503);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        body.ShouldBe(response.StatusCode == HttpStatusCode.OK ? "Healthy" : "Unhealthy");
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/plain");
        response.Headers.CacheControl.ShouldNotBeNull();
        response.Headers.CacheControl.NoStore.ShouldBeTrue();
        response.Headers.CacheControl.NoCache.ShouldBeTrue();
        response.Headers.Pragma.Select(value => value.Name).ShouldContain("no-cache");
        response.Content.Headers.Expires.ShouldBe(DateTimeOffset.UnixEpoch);
        output.WriteLine(JsonSerializer.Serialize(new { path, status = (int)response.StatusCode, body, contentType = response.Content.Headers.ContentType.ToString(), cacheControl = response.Headers.CacheControl.ToString(), expires = response.Content.Headers.Expires }));
        return response.StatusCode;
    }
}

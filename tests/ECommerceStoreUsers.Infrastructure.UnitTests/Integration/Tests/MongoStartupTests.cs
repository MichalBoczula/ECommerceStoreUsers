using ECommerceStoreUsers.Infrastructure.Configuration;
using ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration;
using Shouldly;
using System.Diagnostics;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Tests;

public sealed class MongoStartupTests
{
    private const string UnreachableMongo = "mongodb://127.0.0.1:1/?directConnection=true";

    [Fact]
    public async Task CancelledStartupStopsTheConnectionProbePromptly()
    {
        await using var services = TestServiceProviderFactory.Create(UnreachableMongo, "startup-cancelled");
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var elapsed = Stopwatch.StartNew();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            services.InitializeInfrastructureAsync(cancellation.Token));

        elapsed.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UnreachableMongoFailsWithinTheBoundedConnectionWindow()
    {
        await using var services = TestServiceProviderFactory.Create(UnreachableMongo, "startup-unreachable");
        var elapsed = Stopwatch.StartNew();

        var error = await Should.ThrowAsync<TimeoutException>(() => services.InitializeInfrastructureAsync());

        error.Message.ShouldBe("MongoDB startup connection probe timed out.");
        elapsed.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(16));
    }
}

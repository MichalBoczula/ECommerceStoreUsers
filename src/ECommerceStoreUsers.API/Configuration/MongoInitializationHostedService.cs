using ECommerceStoreUsers.Infrastructure.Configuration;

namespace ECommerceStoreUsers.API.Configuration;

public sealed class MongoInitializationHostedService(
    IServiceProvider services,
    IHostApplicationLifetime lifetime) : IHostedService, IDisposable
{
    private readonly CancellationTokenSource _stop = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var startup = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, lifetime.ApplicationStopping, _stop.Token);
        await services.InitializeInfrastructureAsync(startup.Token);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stop.Cancel();
        return Task.CompletedTask;
    }

    public void Dispose() => _stop.Dispose();
}

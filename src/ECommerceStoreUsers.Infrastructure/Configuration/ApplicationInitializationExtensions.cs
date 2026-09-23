using ECommerceStoreUsers.Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ECommerceStoreUsers.Infrastructure.Configuration
{
    public static class ApplicationInitializationExtensions
    {
        private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(20);
        private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(3);

        public static async Task InitializeInfrastructureAsync(
            this IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
            var mongoInitializer = scope.ServiceProvider.GetRequiredService<MongoInitializer>();
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(StartupTimeout);

            try
            {
                await WaitForConnectionAsync(context.Client, deadline.Token);
                await mongoInitializer.InitializeAsync(deadline.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && deadline.IsCancellationRequested)
            {
                throw new TimeoutException("MongoDB startup initialization exceeded its 20-second limit.");
            }
        }

        private static async Task WaitForConnectionAsync(IMongoClient client, CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var probe = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                probe.CancelAfter(ProbeTimeout);

                try
                {
                    await client.GetDatabase("admin").RunCommandAsync<BsonDocument>(
                        new BsonDocument("ping", 1), cancellationToken: probe.Token);
                    return;
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < 3)
                {
                    // Only the read-only connection probe is safe to repeat.
                }
                catch (MongoServerSelectionException) when (!cancellationToken.IsCancellationRequested && attempt < 3)
                {
                    // No index operation has started yet.
                }
                catch (MongoServerSelectionException) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException("MongoDB startup connection probe timed out.");
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException("MongoDB startup connection probe timed out.");
                }

                if (attempt < 3)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt), cancellationToken);
                }
            }
        }
    }
}

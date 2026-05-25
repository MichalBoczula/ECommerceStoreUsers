using Testcontainers.MongoDb;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration
{
    public sealed class MongoDbTestFixture : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoDbContainer =
            new MongoDbBuilder("mongo:8.0")
                .Build();

        public string ConnectionString => _mongoDbContainer.GetConnectionString();

        public string DatabaseName => "ecommerce-store-users-db";

        public Task InitializeAsync()
        {
            return _mongoDbContainer.StartAsync();
        }

        public Task DisposeAsync()
        {
            return _mongoDbContainer.DisposeAsync().AsTask();
        }
    }
}
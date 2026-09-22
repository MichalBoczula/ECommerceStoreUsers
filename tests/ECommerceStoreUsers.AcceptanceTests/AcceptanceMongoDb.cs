using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace ECommerceStoreUsers.AcceptanceTests
{
    internal static class AcceptanceMongoDb
    {
        private const string Username = "admin";
        private const string Password = "admin123";

        private static MongoDbContainer? _container;
        private static IMongoClient? _client;

        public static string ConnectionString { get; private set; } = string.Empty;

        public static async Task StartAsync()
        {
            if (_container is not null)
            {
                return;
            }

            _container = new MongoDbBuilder("mongo:8.0")
                .WithUsername(Username)
                .WithPassword(Password)
                .WithReplicaSet()
                .Build();

            await _container.StartAsync();

            ConnectionString = _container.GetConnectionString();
            _client = new MongoClient(ConnectionString);
        }

        public static Task DropDatabaseAsync(
            string databaseName,
            CancellationToken cancellationToken = default)
        {
            if (_client is null)
            {
                throw new InvalidOperationException("The acceptance MongoDB fixture has not been started.");
            }

            return _client.DropDatabaseAsync(databaseName, cancellationToken);
        }

        public static async Task DisposeAsync()
        {
            if (_container is null)
            {
                return;
            }

            await _container.DisposeAsync();

            _client = null;
            _container = null;
            ConnectionString = string.Empty;
        }
    }
}

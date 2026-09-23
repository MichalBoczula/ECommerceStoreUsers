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
                throw new InvalidOperationException("The acceptance MongoDB container is already running.");
            }

            var container = new MongoDbBuilder("mongo:8.0")
                .WithUsername(Username)
                .WithPassword(Password)
                .WithReplicaSet()
                .Build();

            try
            {
                await container.StartAsync();
                var connectionString = container.GetConnectionString();
                _client = new MongoClient(connectionString);
                ConnectionString = connectionString;
                _container = container;
            }
            catch
            {
                await container.DisposeAsync();
                throw;
            }
        }

        public static IMongoDatabase GetDatabase(string databaseName)
        {
            ValidateDatabaseName(databaseName);
            if (_client is null)
            {
                throw new InvalidOperationException("The acceptance MongoDB fixture has not been started.");
            }

            return _client.GetDatabase(databaseName);
        }

        public static Task DropDatabaseAsync(
            string databaseName,
            CancellationToken cancellationToken = default)
        {
            ValidateDatabaseName(databaseName);
            if (_client is null)
            {
                throw new InvalidOperationException("The acceptance MongoDB fixture has not been started.");
            }

            return _client.DropDatabaseAsync(databaseName, cancellationToken);
        }

        public static async Task DisposeAsync()
        {
            var container = _container;
            _client = null;
            _container = null;
            ConnectionString = string.Empty;
            if (container is not null)
            {
                await container.DisposeAsync();
            }
        }

        private static void ValidateDatabaseName(string databaseName)
        {
            if (databaseName.Length != 43 ||
                !databaseName.StartsWith("acceptance-", StringComparison.Ordinal) ||
                !Guid.TryParseExact(databaseName[11..], "N", out _))
            {
                throw new ArgumentException("Invalid acceptance database name.", nameof(databaseName));
            }
        }
    }
}

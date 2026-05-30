using System.Reflection;
using ECommerceStoreUsers.API;
using ECommerceStoreUsers.Infrastructure.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MongoDb;

namespace ECommerceStoreUsers.AcceptanceTests
{
    public class ApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private const string Database = "ecommerce-store-users-db-test";
        private const string Username = "admin";
        private const string Password = "admin123";

        private readonly MongoDbContainer _mongoContainer;
        private string _connectionString = string.Empty;

        public ApplicationFactory()
        {
            _mongoContainer = new MongoDbBuilder("mongo:8.0")
                .WithUsername(Username)
                .WithPassword(Password)
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                var overrides = new Dictionary<string, string?>
                {
                    ["MongoDbSettings:ConnectionString"] = _connectionString,
                    ["MongoDbSettings:DatabaseName"] = Database,
                    ["MongoDbSettings:CustomerCollectionName"] = "customers",
                    ["MongoDbSettings:CustomerHistoryCollectionName"] = "customer-history",
                    ["MongoDbSettings:AdminCollectionName"] = "admins"
                };

                config.AddInMemoryCollection(overrides);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<MongoDbContext>();
                services.AddSingleton<MongoDbContext>();
            });
        }

        public async Task InitializeAsync()
        {
            await _mongoContainer.StartAsync();

            var host = _mongoContainer.Hostname;
            var port = _mongoContainer.GetMappedPublicPort(27017);

            _connectionString = $"mongodb://{Username}:{Password}@{host}:{port}/?authSource=admin";

            using var scope = Services.CreateScope();

            var infrastructureAssembly = Assembly.GetAssembly(typeof(MongoDbContext))
                ?? throw new InvalidOperationException("Could not locate Infrastructure assembly.");

            var initializerType = infrastructureAssembly.GetType("ECommerceStoreUsers.Infrastructure.Configuration.MongoInitializer")
                ?? throw new InvalidOperationException("Could not resolve MongoInitializer type.");

            var initializer = ActivatorUtilities.CreateInstance(scope.ServiceProvider, initializerType);

            var initializeMethod = initializerType.GetMethod("InitializeAsync")
                ?? throw new InvalidOperationException("Could not resolve InitializeAsync method.");

            var initializationTask = (Task?)initializeMethod.Invoke(initializer, new object?[] { default(CancellationToken) });

            if (initializationTask is not null)
            {
                await initializationTask;
            }
        }

        public new async Task DisposeAsync()
        {
            await _mongoContainer.DisposeAsync();
        }
    }
}
using ECommerceStoreUsers.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MongoDb;

namespace ECommerceStoreUsers.AcceptanceTests
{
    public class ApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private const string Database = "IntegrationTestDb";
        private const string Username = "root";
        private const string Password = "yourStrong(!)Password";

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
            builder.UseSetting("MongoDbSettings:ConnectionString", _connectionString);
            builder.UseSetting("MongoDbSettings:DatabaseName", Database);
        }

        public async Task InitializeAsync()
        {
            await _mongoContainer.StartAsync();

            var host = _mongoContainer.Hostname;
            var port = _mongoContainer.GetMappedPublicPort(27017);

            _connectionString = $"mongodb://{Username}:{Password}@{host}:{port}/{Database}?authSource=admin";
        }

        public new async Task DisposeAsync()
        {
            await _mongoContainer.DisposeAsync();
        }
    }
}
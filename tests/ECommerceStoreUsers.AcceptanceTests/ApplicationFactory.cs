using ECommerceStoreUsers.API;
using ECommerceStoreUsers.Infrastructure.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerceStoreUsers.AcceptanceTests
{
    public sealed class ApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;
        private readonly string _databaseName;

        public ApplicationFactory(string connectionString, string databaseName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

            _connectionString = connectionString;
            _databaseName = databaseName;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                var overrides = new Dictionary<string, string?>
                {
                    ["MongoDbSettings:ConnectionString"] = _connectionString,
                    ["MongoDbSettings:DatabaseName"] = _databaseName,
                    ["MongoDbSettings:CustomerCollectionName"] = "customers",
                    ["MongoDbSettings:CustomersHistoryCollectionName"] = "customers-history",
                    ["MongoDbSettings:AdminCollectionName"] = "admins",
                    ["MongoDbSettings:AdminsHistoryCollectionName"] = "admins-history",
                    ["MongoDbSettings:FavoriteCollectionName"] = "favorites"
                };

                config.AddInMemoryCollection(overrides);
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<MongoDbContext>();
                services.AddSingleton<MongoDbContext>();
            });
        }
    }
}

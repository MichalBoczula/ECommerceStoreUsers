using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration
{
    internal static class TestServiceProviderFactory
    {
        public static ServiceProvider Create(string mongoConnectionString, string databaseName)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MongoDbSettings:ConnectionString"] = mongoConnectionString,
                    ["MongoDbSettings:DatabaseName"] = databaseName,
                    ["MongoDbSettings:CustomerCollectionName"] = "customers",
                    ["MongoDbSettings:CustomersHistoryCollectionName"] = "customers-history",
                    ["MongoDbSettings:AdminCollectionName"] = "admins",
                    ["MongoDbSettings:AdminsHistoryCollectionName"] = "admins-history"
                })
                .Build();

            var services = new ServiceCollection();

            services.AddInfrastructure(configuration);

            return services.BuildServiceProvider();
        }
    }
}
using ECommerceStoreUsers.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Configuration
{
    public sealed class MongoDbSettingsValidationTests
    {
        [Fact]
        public void AddInfrastructure_WithValidSettings_BindsMongoDbConfiguration()
        {
            var configuration = CreateConfiguration("mongodb://localhost:27017");
            var services = new ServiceCollection();

            services.AddInfrastructure(configuration);

            using var serviceProvider = services.BuildServiceProvider();
            var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;

            settings.ConnectionString.ShouldBe("mongodb://localhost:27017");
            settings.DatabaseName.ShouldBe("ecommerce-store-users-db-test");
            settings.FavoriteCollectionName.ShouldBe("favorites");
        }

        [Fact]
        public void AddInfrastructure_WithoutConnectionString_FailsValidation()
        {
            var configuration = CreateConfiguration(connectionString: null);
            var services = new ServiceCollection();

            services.AddInfrastructure(configuration);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>();

            var exception = Should.Throw<OptionsValidationException>(() => _ = options.Value);
            exception.Failures.ShouldContain(
                "MongoDbSettings:ConnectionString must be configured.");
        }

        [Theory]
        [InlineData("http://localhost:27017")]
        [InlineData("localhost:27017")]
        public void AddInfrastructure_WithUnsupportedConnectionStringScheme_FailsValidation(
            string connectionString)
        {
            var configuration = CreateConfiguration(connectionString);
            var services = new ServiceCollection();

            services.AddInfrastructure(configuration);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>();

            var exception = Should.Throw<OptionsValidationException>(() => _ = options.Value);
            exception.Failures.ShouldContain(
                "MongoDbSettings:ConnectionString must use the mongodb:// or mongodb+srv:// scheme.");
        }

        private static IConfiguration CreateConfiguration(string? connectionString)
        {
            var values = new Dictionary<string, string?>
            {
                ["MongoDbSettings:DatabaseName"] = "ecommerce-store-users-db-test",
                ["MongoDbSettings:CustomerCollectionName"] = "customers",
                ["MongoDbSettings:CustomersHistoryCollectionName"] = "customers-history",
                ["MongoDbSettings:AdminCollectionName"] = "admins",
                ["MongoDbSettings:AdminsHistoryCollectionName"] = "admins-history",
                ["MongoDbSettings:FavoriteCollectionName"] = "favorites"
            };

            if (connectionString is not null)
            {
                values["MongoDbSettings:ConnectionString"] = connectionString;
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }
    }
}

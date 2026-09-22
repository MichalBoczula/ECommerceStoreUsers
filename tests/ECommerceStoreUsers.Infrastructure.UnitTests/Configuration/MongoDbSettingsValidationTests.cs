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
            var configuration = CreateConfiguration();
            var services = new ServiceCollection();

            services.AddInfrastructure(configuration);

            using var serviceProvider = services.BuildServiceProvider();
            var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;

            settings.ConnectionString.ShouldBe("mongodb://localhost:27017");
            settings.DatabaseName.ShouldBe("ecommerce-store-users-db-test");
            settings.FavoriteCollectionName.ShouldBe("favorites");
        }

        [Theory]
        [InlineData(
            "MongoDbSettings:ConnectionString",
            "MongoDbSettings:ConnectionString must be configured.")]
        [InlineData(
            "MongoDbSettings:DatabaseName",
            "MongoDbSettings:DatabaseName must be configured.")]
        [InlineData(
            "MongoDbSettings:CustomerCollectionName",
            "MongoDbSettings:CustomerCollectionName must be configured.")]
        [InlineData(
            "MongoDbSettings:CustomersHistoryCollectionName",
            "MongoDbSettings:CustomersHistoryCollectionName must be configured.")]
        [InlineData(
            "MongoDbSettings:AdminCollectionName",
            "MongoDbSettings:AdminCollectionName must be configured.")]
        [InlineData(
            "MongoDbSettings:AdminsHistoryCollectionName",
            "MongoDbSettings:AdminsHistoryCollectionName must be configured.")]
        [InlineData(
            "MongoDbSettings:FavoriteCollectionName",
            "MongoDbSettings:FavoriteCollectionName must be configured.")]
        public void AddInfrastructure_WithoutRequiredSetting_FailsValidation(
            string missingSetting,
            string expectedFailure)
        {
            var configuration = CreateConfiguration(excludedSetting: missingSetting);
            var services = new ServiceCollection();

            services.AddInfrastructure(configuration);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>();

            var exception = Should.Throw<OptionsValidationException>(() => _ = options.Value);
            exception.Failures.ShouldContain(expectedFailure);
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

        private static IConfiguration CreateConfiguration(
            string connectionString = "mongodb://localhost:27017",
            string? excludedSetting = null)
        {
            var values = new Dictionary<string, string?>
            {
                ["MongoDbSettings:ConnectionString"] = connectionString,
                ["MongoDbSettings:DatabaseName"] = "ecommerce-store-users-db-test",
                ["MongoDbSettings:CustomerCollectionName"] = "customers",
                ["MongoDbSettings:CustomersHistoryCollectionName"] = "customers-history",
                ["MongoDbSettings:AdminCollectionName"] = "admins",
                ["MongoDbSettings:AdminsHistoryCollectionName"] = "admins-history",
                ["MongoDbSettings:FavoriteCollectionName"] = "favorites"
            };

            if (excludedSetting is not null)
            {
                values.Remove(excludedSetting);
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }
    }
}

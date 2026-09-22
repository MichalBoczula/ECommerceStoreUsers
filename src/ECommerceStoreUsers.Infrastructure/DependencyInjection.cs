using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Infrastructure.Configuration;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceStoreUsers.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddOptions<MongoDbSettings>()
                .Bind(configuration.GetSection(MongoDbSettings.SectionName))
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.ConnectionString),
                    "MongoDbSettings:ConnectionString must be configured.")
                .Validate(
                    settings =>
                        string.IsNullOrWhiteSpace(settings.ConnectionString)
                        || settings.ConnectionString.StartsWith("mongodb://", StringComparison.OrdinalIgnoreCase)
                        || settings.ConnectionString.StartsWith("mongodb+srv://", StringComparison.OrdinalIgnoreCase),
                    "MongoDbSettings:ConnectionString must use the mongodb:// or mongodb+srv:// scheme.")
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.DatabaseName),
                    "MongoDbSettings:DatabaseName must be configured.")
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.CustomerCollectionName),
                    "MongoDbSettings:CustomerCollectionName must be configured.")
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.CustomersHistoryCollectionName),
                    "MongoDbSettings:CustomersHistoryCollectionName must be configured.")
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.AdminCollectionName),
                    "MongoDbSettings:AdminCollectionName must be configured.")
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.AdminsHistoryCollectionName),
                    "MongoDbSettings:AdminsHistoryCollectionName must be configured.")
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.FavoriteCollectionName),
                    "MongoDbSettings:FavoriteCollectionName must be configured.")
                .ValidateOnStart();

            services.AddSingleton<MongoDbContext>();
            services.AddScoped<MongoInitializer>();

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();

            return services;
        }
    }
}

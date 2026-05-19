using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
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
            services.Configure<MongoDbSettings>(
                configuration.GetSection(MongoDbSettings.SectionName));

            services.AddSingleton<MongoDbContext>();
            services.AddScoped<MongoInitializer>();

            services.AddScoped<ICustomerRepository, CustomerRepository>();

            return services;
        }
    }
}

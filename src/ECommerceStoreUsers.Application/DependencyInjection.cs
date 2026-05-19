using ECommerceStoreUsers.Application.Services.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceStoreUsers.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<ICustomerService, CustomerService>();
            return services;
        }
    }
}

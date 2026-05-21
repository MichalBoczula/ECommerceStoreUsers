using ECommerceStoreUsers.Application.Services.Abstract.Customers;
using ECommerceStoreUsers.Application.Services.Concrete.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceStoreUsers.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerDescriptorService, CustomerDescriptorService>();
            return services;
        }
    }
}

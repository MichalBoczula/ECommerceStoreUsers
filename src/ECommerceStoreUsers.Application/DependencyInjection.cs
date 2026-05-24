using ECommerceStoreUsers.Application.Services.Abstract.Admins;
using ECommerceStoreUsers.Application.Services.Abstract.Customers;
using ECommerceStoreUsers.Application.Services.Concrete.Admins;
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

            services.AddScoped<IAdminProfileService, AdminProfileService>();
            services.AddScoped<IAdminFlowDescriptorService, AdminFlowDescriptorService>();
            return services;
        }
    }
}

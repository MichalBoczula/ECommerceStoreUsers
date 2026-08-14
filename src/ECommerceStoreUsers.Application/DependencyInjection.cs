using ECommerceStoreUsers.Application.Services.Abstract.Admins;
using ECommerceStoreUsers.Application.Services.Abstract.Customers;
using ECommerceStoreUsers.Application.Services.Abstract.Favorites;
using ECommerceStoreUsers.Application.Services.Concrete.Admins;
using ECommerceStoreUsers.Application.Services.Concrete.Customers;
using ECommerceStoreUsers.Application.Services.Concrete.Favorites;
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

            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IFavoriteFlowDescriptorService, FavoriteFlowDescriptorService>();
            return services;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace ECommerceStoreUsers.Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDomain(
           this IServiceCollection services)
        {
            return services;
        }
    }
}

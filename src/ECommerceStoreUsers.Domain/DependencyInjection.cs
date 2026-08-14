using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Employees.Admins;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Favorites;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceStoreUsers.Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDomain(
           this IServiceCollection services)
        {
            services.AddScoped<IValidationPolicy<Customer>, CustomerValidationPolicy>();
            services.AddScoped<IValidationPolicyDescriptorProvider, CustomerValidationPolicy>();
            services.AddScoped<IValidationPolicy<IndividualData>, IndividualDataValidationPolicy>();
            services.AddScoped<IValidationPolicyDescriptorProvider, IndividualDataValidationPolicy>();
            services.AddScoped<IValidationPolicy<CompanyData>, CompanyValidationPolicy>();
            services.AddScoped<IValidationPolicyDescriptorProvider, CompanyValidationPolicy>();
            services.AddScoped<IValidationPolicy<Admin>, AdminValidationPolicy>();
            services.AddScoped<IValidationPolicyDescriptorProvider, AdminValidationPolicy>();
            services.AddScoped<IValidationPolicy<Guid>, EmptyGuidValidationPolicy>();
            services.AddScoped<IValidationPolicyDescriptorProvider, EmptyGuidValidationPolicy>();
            services.AddScoped<IValidationPolicy<Favorite>, FavoriteValidationPolicy>();
            services.AddScoped<IValidationPolicyDescriptorProvider, FavoriteValidationPolicy>();

            return services;
        }
    }
}

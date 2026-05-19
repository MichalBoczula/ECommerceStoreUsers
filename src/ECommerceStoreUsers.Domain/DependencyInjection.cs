using Microsoft.Extensions.DependencyInjection;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers.Addresses;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers.Customers;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers.Entities.CompanyDatas;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers.Entities.IndividualDatas;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Employees.Admins;

namespace ECommerceStoreUsers.Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDomain(
           this IServiceCollection services)
        {
            services.AddScoped<IValidationPolicy<Customer>, CustomerValidationPolicy>();
            services.AddScoped<IValidationPolicyDescriptorProvider, CustomerValidationPolicy>();
            services.AddScoped<IValidationPolicy<Address>, AddressValidationPolicy>();
            services.AddScoped<IValidationPolicyDescriptorProvider, AddressValidationPolicy>();
            services.AddScoped<IValidationPolicy<IndividualData>, IndividualDataValidationPolicy>();
            services.AddScoped<IValidationPolicyDescriptorProvider, IndividualDataValidationPolicy>();
            services.AddScoped<IValidationPolicy<CompanyData>, CompanyDataValidationPolicy>();
            services.AddScoped<IValidationPolicyDescriptorProvider, CompanyDataValidationPolicy>();
            services.AddScoped<IValidationPolicy<Admin>, AdminValidationPolicy>();
            services.AddScoped<IValidationPolicyDescriptorProvider, AdminValidationPolicy>();

            return services;
        }
    }
}

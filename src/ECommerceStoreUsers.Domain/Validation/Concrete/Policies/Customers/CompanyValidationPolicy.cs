using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.Addresses;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.CompanyDatas;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers;

internal sealed class CompanyValidationPolicy : IValidationPolicy<CompanyData>, IValidationPolicyDescriptorProvider
{
    private readonly List<IValidationRule<CompanyData>> _companyRules = [];
    private readonly List<IValidationRule<Address>> _addressRules = [];

    public CompanyValidationPolicy()
    {
        _companyRules.Add(new CompanyDataCompanyNameValidationRule());
        _companyRules.Add(new CompanyDataTaxIdValidationRule());

        _addressRules.Add(new AddressPostalCodeValidationRule());
        _addressRules.Add(new AddressCityValidationRule());
        _addressRules.Add(new AddressStreetValidationRule());
        _addressRules.Add(new AddressBuildingNumberValidationRule());
        _addressRules.Add(new AddressApartmentNumberValidationRule());
    }

    public async Task<ValidationResult> Validate(CompanyData entity)
    {
        ValidationResult validationResult = new();

        foreach (var rule in _companyRules)
            await rule.IsValid(entity, validationResult);

        foreach (var rule in _addressRules)
        {
            await rule.IsValid(entity.BillingAddress, validationResult);
            await rule.IsValid(entity.ShippingAddress, validationResult);
        }

        return validationResult;
    }

    public ValidationPolicyDescriptor Describe()
    {
        var descriptors = new List<ValidationRuleDescriptor>();

        descriptors.AddRange(_companyRules.Select(r => new ValidationRuleDescriptor { RuleName = r.GetType().Name, Rules = r.Describe() }));
        descriptors.AddRange(_addressRules.Select(r => new ValidationRuleDescriptor { RuleName = r.GetType().Name, Rules = r.Describe() }));

        return new ValidationPolicyDescriptor
        {
            PolicyName = nameof(CompanyValidationPolicy),
            Rules = descriptors
        };
    }
}

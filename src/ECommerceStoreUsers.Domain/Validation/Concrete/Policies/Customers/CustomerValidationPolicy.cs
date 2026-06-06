using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.Addresses;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.CompanyDatas;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.IndividualDatas;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Customers;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers;

internal sealed class CustomerValidationPolicy : IValidationPolicy<Customer>, IValidationPolicyDescriptorProvider
{
    private readonly List<IValidationRule<Customer>> _customerRules = [];
    private readonly List<IValidationRule<IndividualData>> _individualRules = [];
    private readonly List<IValidationRule<CompanyData>> _companyRules = [];
    private readonly List<IValidationRule<IReadOnlyCollection<CompanyData>>> _companyCollectionRules = [];
    private readonly List<IValidationRule<Address>> _addressRules = [];

    public CustomerValidationPolicy()
    {
        _customerRules.Add(new CustomerExternalIdValidationRule());
        _companyCollectionRules.Add(new CompanyDataTaxIdUniquenessValidationRule());

        _individualRules.Add(new IndividualDataFirstNameValidationRule());
        _individualRules.Add(new IndividualDataLastNameValidationRule());
        _individualRules.Add(new IndividualDataEmailValidationRule());
        _individualRules.Add(new IndividualDataPhoneValidationRule());

        _companyRules.Add(new CompanyDataCompanyNameValidationRule());
        _companyRules.Add(new CompanyDataTaxIdValidationRule());

        _addressRules.Add(new AddressPostalCodeValidationRule());
        _addressRules.Add(new AddressCityValidationRule());
        _addressRules.Add(new AddressStreetValidationRule());
        _addressRules.Add(new AddressBuildingNumberValidationRule());
        _addressRules.Add(new AddressApartmentNumberValidationRule());
    }

    public async Task<ValidationResult> Validate(Customer entity)
    {
        ValidationResult validationResult = new();

        foreach (var rule in _customerRules)
            await rule.IsValid(entity, validationResult);

        if (entity.Individual is not null)
        {
            foreach (var rule in _individualRules)
                await rule.IsValid(entity.Individual, validationResult);

            foreach (var rule in _addressRules)
            {
                await rule.IsValid(entity.Individual.BillingAddress, validationResult);
                await rule.IsValid(entity.Individual.ShippingAddress, validationResult);
            }
        }

        foreach (var rule in _companyCollectionRules)
            await rule.IsValid(entity.Companies, validationResult);

        foreach (var company in entity.Companies)
        {
            foreach (var rule in _companyRules)
                await rule.IsValid(company, validationResult);

            foreach (var rule in _addressRules)
            {
                await rule.IsValid(company.BillingAddress, validationResult);
                await rule.IsValid(company.ShippingAddress, validationResult);
            }
        }

        return validationResult;
    }

    public ValidationPolicyDescriptor Describe()
    {
        var descriptors = new List<ValidationRuleDescriptor>();

        descriptors.AddRange(_customerRules.Select(r => new ValidationRuleDescriptor { RuleName = r.GetType().Name, Rules = r.Describe() }));
        descriptors.AddRange(_individualRules.Select(r => new ValidationRuleDescriptor { RuleName = r.GetType().Name, Rules = r.Describe() }));
        descriptors.AddRange(_companyRules.Select(r => new ValidationRuleDescriptor { RuleName = r.GetType().Name, Rules = r.Describe() }));
        descriptors.AddRange(_companyCollectionRules.Select(r => new ValidationRuleDescriptor { RuleName = r.GetType().Name, Rules = r.Describe() }));
        descriptors.AddRange(_addressRules.Select(r => new ValidationRuleDescriptor { RuleName = r.GetType().Name, Rules = r.Describe() }));

        return new ValidationPolicyDescriptor
        {
            PolicyName = nameof(CustomerValidationPolicy),
            Rules = descriptors
        };
    }
}
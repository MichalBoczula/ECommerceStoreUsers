using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Addresses;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers.Addresses
{
    internal sealed class AddressValidationPolicy : IValidationPolicy<Address>, IValidationPolicyDescriptorProvider
    {
        private readonly List<IValidationRule<Address>> _rules = [];

        public AddressValidationPolicy()
        {
            _rules.Add(new AddressPostalCodeValidationRule());
            _rules.Add(new AddressCityValidationRule());
            _rules.Add(new AddressStreetValidationRule());
            _rules.Add(new AddressBuildingNumberValidationRule());
            _rules.Add(new AddressApartmentNumberValidationRule());
        }

        public async Task<ValidationResult> Validate(Address entity)
        {
            ValidationResult validationResult = new();

            foreach (var rule in _rules)
                await rule.IsValid(entity, validationResult);

            return validationResult;
        }

        public ValidationPolicyDescriptor Describe()
        {
            var descriptors = _rules
                .Select(rule => new ValidationRuleDescriptor
                {
                    RuleName = rule.GetType().Name,
                    Rules = rule.Describe()
                })
                .ToList();

            return new ValidationPolicyDescriptor
            {
                PolicyName = nameof(AddressValidationPolicy),
                Rules = descriptors
            };
        }
    }
}
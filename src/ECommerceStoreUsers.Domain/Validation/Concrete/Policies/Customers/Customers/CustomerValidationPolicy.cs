using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Customers;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers.Customers
{
    internal sealed class CustomerValidationPolicy : IValidationPolicy<Customer>, IValidationPolicyDescriptorProvider
    {
        private readonly List<IValidationRule<Customer>> _rules = [];

        public CustomerValidationPolicy()
        {
            _rules.Add(new CustomerExternalIdValidationRule());
        }

        public async Task<ValidationResult> Validate(Customer entity)
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
                PolicyName = nameof(CustomerValidationPolicy),
                Rules = descriptors
            };
        }
    }
}

using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Entities.IndividualDatas;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers.Entities.IndividualDatas
{
    internal sealed class IndividualDataValidationPolicy : IValidationPolicy<IndividualData>, IValidationPolicyDescriptorProvider
    {
        private readonly List<IValidationRule<IndividualData>> _rules = [];

        public IndividualDataValidationPolicy()
        {
            _rules.Add(new IndividualDataFirstNameValidationRule());
            _rules.Add(new IndividualDataLastNameValidationRule());
            _rules.Add(new IndividualDataEmailValidationRule());
            _rules.Add(new IndividualDataPhoneValidationRule());
        }

        public async Task<ValidationResult> Validate(IndividualData entity)
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
                PolicyName = nameof(IndividualDataValidationPolicy),
                Rules = descriptors
            };
        }
    }
}

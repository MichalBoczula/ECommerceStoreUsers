using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Entities.CompanyDatas;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers.Entities.CompanyDatas
{
    internal sealed class CompanyDataValidationPolicy : IValidationPolicy<CompanyData>, IValidationPolicyDescriptorProvider
    {
        private readonly List<IValidationRule<CompanyData>> _rules = [];

        public CompanyDataValidationPolicy()
        {
            _rules.Add(new CompanyDataCompanyNameValidationRule());
            _rules.Add(new CompanyDataTaxIdValidationRule());
        }

        public async Task<ValidationResult> Validate(CompanyData entity)
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
                PolicyName = nameof(CompanyDataValidationPolicy),
                Rules = descriptors
            };
        }
    }
}

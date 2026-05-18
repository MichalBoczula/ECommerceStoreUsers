using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Employees.Admins;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Employees.Admins
{
    internal sealed class AdminValidationPolicy : IValidationPolicy<Admin>, IValidationPolicyDescriptorProvider
    {
        private readonly List<IValidationRule<Admin>> _rules = [];

        public AdminValidationPolicy()
        {
            _rules.Add(new AdminExternalIdValidationRule());
            _rules.Add(new AdminFullNameValidationRule());
            _rules.Add(new AdminEmailValidationRule());
        }

        public async Task<ValidationResult> Validate(Admin entity)
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
                PolicyName = nameof(AdminValidationPolicy),
                Rules = descriptors
            };
        }
    }
}
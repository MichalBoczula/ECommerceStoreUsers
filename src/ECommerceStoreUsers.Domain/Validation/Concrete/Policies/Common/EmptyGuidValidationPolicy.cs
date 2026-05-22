using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Common;

internal sealed class EmptyGuidValidationPolicy : IValidationPolicy<Guid>, IValidationPolicyDescriptorProvider
{
    private readonly List<IValidationRule<Guid>> _rules = [];

    public EmptyGuidValidationPolicy()
    {
        _rules.Add(new EmptyGuidRule());
    }

    public async Task<ValidationResult> Validate(Guid entity)
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
            PolicyName = nameof(EmptyGuidValidationPolicy),
            Rules = descriptors
        };
    }
}

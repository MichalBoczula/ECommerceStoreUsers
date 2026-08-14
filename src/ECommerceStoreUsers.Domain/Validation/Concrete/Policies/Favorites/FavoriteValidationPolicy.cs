using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Favorites;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Favorites
{
    internal sealed class FavoriteValidationPolicy : IValidationPolicy<Favorite>, IValidationPolicyDescriptorProvider
    {
        private readonly List<IValidationRule<Favorite>> _rules = [];

        public FavoriteValidationPolicy()
        {
            _rules.Add(new FavoriteClientIdValidationRule());
            _rules.Add(new FavoriteProductIdValidationRule());
        }

        public async Task<ValidationResult> Validate(Favorite entity)
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
                PolicyName = nameof(FavoriteValidationPolicy),
                Rules = descriptors
            };
        }
    }
}

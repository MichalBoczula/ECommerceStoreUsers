using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Favorites
{
    internal sealed class FavoriteProductIdValidationRule : IValidationRule<Favorite>
    {
        private readonly ValidationError _invalidProductId;

        public FavoriteProductIdValidationRule()
        {
            _invalidProductId = new ValidationError
            {
                Message = "ProductId cannot be an empty GUID.",
                Name = nameof(FavoriteProductIdValidationRule),
                Entity = nameof(Favorite)
            };
        }

        public Task IsValid(Favorite entity, ValidationResult validationResults)
        {
            if (entity is null) return Task.CompletedTask;

            if (entity.ProductId == Guid.Empty)
                validationResults.AddValidationError(_invalidProductId);

            return Task.CompletedTask;
        }

        public List<ValidationError> Describe() => [_invalidProductId];
    }
}

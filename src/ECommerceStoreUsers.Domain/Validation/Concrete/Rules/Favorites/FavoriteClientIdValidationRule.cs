using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Favorites
{
    internal sealed class FavoriteClientIdValidationRule : IValidationRule<Favorite>
    {
        private readonly ValidationError _invalidClientId;

        public FavoriteClientIdValidationRule()
        {
            _invalidClientId = new ValidationError
            {
                Message = "ClientId cannot be an empty GUID.",
                Name = nameof(FavoriteClientIdValidationRule),
                Entity = nameof(Favorite)
            };
        }

        public Task IsValid(Favorite entity, ValidationResult validationResults)
        {
            if (entity is null) return Task.CompletedTask;

            if (entity.ClientId == Guid.Empty)
                validationResults.AddValidationError(_invalidClientId);

            return Task.CompletedTask;
        }

        public List<ValidationError> Describe() => [_invalidClientId];
    }
}

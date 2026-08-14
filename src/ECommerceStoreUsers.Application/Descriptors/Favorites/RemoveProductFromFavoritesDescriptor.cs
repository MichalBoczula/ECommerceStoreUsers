using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Favorites
{
    internal sealed record RemoveProductFromFavorites;

    internal sealed class RemoveProductFromFavoritesDescriptor : FlowDescriberBase<RemoveProductFromFavorites>
    {
        [FlowStep(order: 1, bpmnId: "ValidateIdentifiers")]
        public async Task<ValidationResult> ValidateIdentifiers(
            Guid clientId,
            Guid productId,
            IValidationPolicy<Guid> emptyGuidValidationPolicy)
        {
            var clientValidation = await emptyGuidValidationPolicy.Validate(clientId);
            var productValidation = await emptyGuidValidationPolicy.Validate(productId);

            var result = new ValidationResult();
            foreach (var error in clientValidation.GetValidationErrors()) result.AddValidationError(error);
            foreach (var error in productValidation.GetValidationErrors()) result.AddValidationError(error);

            return result;
        }

        [FlowStep(order: 2, bpmnId: "AreIdentifiersValid")]
        public void ThrowValidationExceptionIfIdentifiersInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 3, bpmnId: "LoadFavorite")]
        public async Task<Favorite?> LoadFavorite(Guid clientId, Guid productId, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            return await favoriteRepository.GetByClientAndProductIdAsync(clientId, productId, cancellationToken);
        }

        [FlowStep(order: 4, bpmnId: "VerifyFavoriteExists")]
        public void ThrowNotFoundExceptionIfFavoriteMissing(Guid clientId, Guid productId, Favorite? favorite)
        {
            if (favorite is null)
            {
                throw new ResourceNotFoundException(
                    nameof(RemoveProductFromFavorites),
                    $"{clientId}:{productId}",
                    nameof(Favorite));
            }
        }

        [FlowStep(order: 5, bpmnId: "DeleteFavorite")]
        public async Task Delete(Guid clientId, Guid productId, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            await favoriteRepository.DeleteAsync(clientId, productId, cancellationToken);
        }
    }
}

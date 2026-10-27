using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Favorites
{
    internal sealed record RemoveProductFromFavorites;

    [Documentation("users.flow.remove-product-from-favorites", "Delete one exact Favorite pair, distinguishing missing-at-read from delete-missed races.")]
    [FlowOutcome("users.branch.remove-product-from-favorites.removed", "Valid identifiers locate a Favorite and the exact-pair delete removes it.", "Return bodyless 204; preserve every unrelated Favorite and add no history.", Status = 204, Stage = "application")]
    [FlowOutcome("users.branch.remove-product-from-favorites.missing", "The exact client/product pair is absent during the initial read.", "Return 404 resource_not_found without deleting other entries.", Status = 404, Code = "resource_not_found", Stage = "application")]
    [FlowOutcome("users.branch.remove-product-from-favorites.delete-missed", "The pair existed at read time but another write removes it before DeleteOne.", "Return 404 resource_not_found; retain the committed competing outcome.", Status = 404, Code = "resource_not_found", Stage = "persistence")]
    [FlowOutcome("users.branch.remove-product-from-favorites.validation", "An invoked policy collects errors and its validation guard rejects them.", "Return 400 validation_failed with complete rule errors and no writes.", "users.rule.empty-guid", Status = 400, Code = "validation_failed", Stage = "application")]
    internal sealed class RemoveProductFromFavoritesDescriptor : FlowDescriberBase<RemoveProductFromFavorites>
    {
        [FlowStep(order: 1, bpmnId: "ValidateIdentifiers", description: "Validate both clientId and productId with EmptyGuidValidationPolicy and collect both results; no repository call or write occurs.")]
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

        [FlowStep(order: 2, bpmnId: "AreIdentifiersValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfIdentifiersInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 3, bpmnId: "LoadFavorite", description: "Read the exact client/product pair; null is handled by the following 404 guard. No write occurs.")]
        public async Task<Favorite?> LoadFavorite(Guid clientId, Guid productId, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            return await favoriteRepository.GetByClientAndProductIdAsync(clientId, productId, cancellationToken);
        }

        [FlowStep(order: 4, bpmnId: "VerifyFavoriteExists", description: "Continue when the requested favorite exists; otherwise throw 404 resource_not_found before persistence.")]
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

        [FlowStep(order: 5, bpmnId: "DeleteFavorite", description: "Issue one exact-pair DeleteOne and return whether it removed a document; the next guard distinguishes a missed delete. This is not a current/history transaction.")]
        public async Task<bool> Delete(Guid clientId, Guid productId, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            return await favoriteRepository.DeleteAsync(clientId, productId, cancellationToken);
        }

        [FlowStep(order: 6, bpmnId: "VerifyFavoriteWasDeleted", description: "Continue only when DeleteOne removed the requested pair; a concurrent removal yields 404 resource_not_found. Favorites have no history.")]
        public void ThrowNotFoundExceptionIfDeleteMissed(Guid clientId, Guid productId, bool deleted)
        {
            if (!deleted)
            {
                throw new ResourceNotFoundException(
                    nameof(RemoveProductFromFavorites),
                    $"{clientId}:{productId}",
                    nameof(Favorite));
            }
        }
    }
}


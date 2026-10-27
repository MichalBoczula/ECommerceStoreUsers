using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Favorites
{
    internal sealed record ClearClientFavorites;

    [Documentation("users.flow.clear-client-favorites", "Delete one client's Favorites; an empty match is a successful bodyless no-op.")]
    [FlowOutcome("users.branch.clear-client-favorites.cleared", "The client UUID is valid, whether matching Favorites exist or not.", "Delete that client's Favorites or perform a no-op; return bodyless 204 without history.", Status = 204, Stage = "application")]
    [FlowOutcome("users.branch.clear-client-favorites.validation", "An invoked policy collects errors and its validation guard rejects them.", "Return 400 validation_failed with complete rule errors and no writes.", "users.rule.empty-guid", Status = 400, Code = "validation_failed", Stage = "application")]
    internal sealed class ClearClientFavoritesDescriptor : FlowDescriberBase<ClearClientFavorites>
    {
        [FlowStep(order: 1, bpmnId: "ValidateClientId", description: "Run EmptyGuidValidationPolicy for ClientId; collect errors for the next guard without checking resource existence.")]
        public async Task<ValidationResult> ValidateClientId(Guid clientId, IValidationPolicy<Guid> emptyGuidValidationPolicy)
        {
            return await emptyGuidValidationPolicy.Validate(clientId);
        }

        [FlowStep(order: 2, bpmnId: "IsClientIdValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfClientIdInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 3, bpmnId: "ClearAllFavorites", description: "Delete only this client UUID's Favorites; zero matches are a successful no-op. The endpoint returns bodyless 204; no Favorite history exists.")]
        public async Task ClearAll(Guid clientId, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            await favoriteRepository.DeleteAllByClientIdAsync(clientId, cancellationToken);
        }
    }
}


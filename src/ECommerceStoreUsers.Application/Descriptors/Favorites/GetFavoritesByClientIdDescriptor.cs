using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Favorites;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Favorites
{
    internal sealed record GetFavoritesByClientId;

    [Documentation("users.flow.get-favorites-by-client-id", "Read one client's Favorite entries in timestamp-descending order, including an empty list.")]
    [FlowOutcome("users.branch.get-favorites-by-client-id.read", "The input is valid and the requested result is available.", "Return the complete read-only response with no stored changes.", Status = 200, Stage = "application")]
    [FlowOutcome("users.branch.get-favorites-by-client-id.validation", "An invoked policy collects errors and its validation guard rejects them.", "Return 400 validation_failed with complete rule errors and no writes. GET external-ID validation uses synthetic valid non-key fields.", "users.rule.empty-guid", Status = 400, Code = "validation_failed", Stage = "application")]
    internal sealed class GetFavoritesByClientIdDescriptor : FlowDescriberBase<GetFavoritesByClientId>
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

        [FlowStep(order: 3, bpmnId: "LoadFavoritesByClientId", description: "Read only this client UUID's Favorite entries sorted by AddedAt descending; no matches produce [] and no tie-breaker or history is added.")]
        public async Task<IReadOnlyList<Favorite>> LoadFavorites(Guid clientId, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            return await favoriteRepository.GetByClientIdAsync(clientId, cancellationToken);
        }

        [FlowStep(order: 4, bpmnId: "MapFavoritesResponse", description: "Map the Favorite list, including [] for no matches, in repository AddedAt-descending order without promising a tie-breaker; this mapping adds no database writes or history.")]
        public IReadOnlyList<FavoriteResponseDto> MapToResponse(IReadOnlyList<Favorite> favorites)
        {
            return favorites.Select(MappingConfig.MapToResponse).ToList();
        }
    }
}


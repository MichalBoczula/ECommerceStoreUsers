using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.RequestsDto.Favorites;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Favorites;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Favorites
{
    internal sealed record AddProductToFavorites;

    [Documentation("users.flow.add-product-to-favorites", "Insert a unique client/product Favorite without aggregate history or cross-service existence checks.")]
    [FlowOutcome("users.branch.add-product-to-favorites.saved", "Valid input passes checks and the repository commits the requested write.", "Return the complete 200 Favorite; write only one Favorite document, without history.", Status = 200, Stage = "application")]
    [FlowOutcome("users.branch.add-product-to-favorites.pair-exists", "The pair already exists at precheck or a competing insert wins its unique index.", "Return 409 resource_conflict and preserve the one stored pair; no history.", Status = 409, Code = "resource_conflict", Stage = "application")]
    [FlowOutcome("users.branch.add-product-to-favorites.validation", "An invoked policy collects errors and its validation guard rejects them.", "Return 400 validation_failed with complete rule errors and no writes.", "users.rule.favorite-client-id", "users.rule.favorite-product-id", Status = 400, Code = "validation_failed", Stage = "application")]
    internal sealed class AddProductToFavoritesDescriptor : FlowDescriberBase<AddProductToFavorites>
    {
        [FlowStep(order: 1, bpmnId: "MapRequestToDomain", description: "Build an in-memory Favorite with the supplied client/product pair and generated ID/time; nothing is saved yet.")]
        public Favorite MapToDomain(Guid clientId, AddFavoriteRequestDto request)
        {
            return MappingConfig.MapToDomain(clientId, request);
        }

        [FlowStep(order: 2, bpmnId: "ValidateFavoriteAggregate", description: "Run FavoriteValidationPolicy and collect its active rules/errors without writing; the next validation guard decides whether to return a safe failure.")]
        public async Task<ValidationResult> ValidateFavorite(Favorite favorite, IValidationPolicy<Favorite> favoriteValidationPolicy)
        {
            return await favoriteValidationPolicy.Validate(favorite);
        }

        [FlowStep(order: 3, bpmnId: "IsFavoriteAggregateValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfFavoriteInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 4, bpmnId: "CheckIfAlreadyFavorited", description: "Read whether the exact client/product pair exists. The next guard rejects duplicates; the unique index still handles inserts racing after this check.")]
        public async Task<bool> CheckExists(Guid clientId, Guid productId, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            return await favoriteRepository.ExistsAsync(clientId, productId, cancellationToken);
        }

        [FlowStep(order: 5, bpmnId: "IsProductAlreadyFavorited", description: "Continue when the client/product pair is not already favorited; otherwise return 409 resource_conflict before the write. The repository separately guards races.")]
        public void ThrowAlreadyExistsExceptionIfFavorited(Guid clientId, Guid productId, bool exists)
        {
            if (exists)
            {
                throw new ResourceAlreadyExistsException(
                    nameof(AddProductToFavorites),
                    $"{clientId}:{productId}",
                    nameof(Favorite));
            }
        }

        [FlowStep(order: 6, bpmnId: "SaveFavorite", description: "Insert one Favorite under the unique client/product index, without history. A confirmed conflicting pair maps to 409 resource_conflict; unrelated failures remain safe 500 errors.")]
        public async Task Save(Favorite favorite, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            await favoriteRepository.AddAsync(favorite, cancellationToken);
        }

        [FlowStep(order: 7, bpmnId: "MapFavoriteResponse", description: "Map the current Favorite into the complete response DTO; this mapping adds no database writes or history.")]
        public FavoriteResponseDto MapToResponse(Favorite favorite)
        {
            return MappingConfig.MapToResponse(favorite);
        }
    }
}


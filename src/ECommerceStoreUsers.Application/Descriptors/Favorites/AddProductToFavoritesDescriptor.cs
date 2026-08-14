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

    internal sealed class AddProductToFavoritesDescriptor : FlowDescriberBase<AddProductToFavorites>
    {
        [FlowStep(order: 1, bpmnId: "MapRequestToDomain")]
        public Favorite MapToDomain(Guid clientId, AddFavoriteRequestDto request)
        {
            return MappingConfig.MapToDomain(clientId, request);
        }

        [FlowStep(order: 2, bpmnId: "ValidateFavoriteAggregate")]
        public async Task<ValidationResult> ValidateFavorite(Favorite favorite, IValidationPolicy<Favorite> favoriteValidationPolicy)
        {
            return await favoriteValidationPolicy.Validate(favorite);
        }

        [FlowStep(order: 3, bpmnId: "IsFavoriteAggregateValid")]
        public void ThrowValidationExceptionIfFavoriteInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 4, bpmnId: "CheckIfAlreadyFavorited")]
        public async Task<bool> CheckExists(Guid clientId, Guid productId, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            return await favoriteRepository.ExistsAsync(clientId, productId, cancellationToken);
        }

        [FlowStep(order: 5, bpmnId: "IsProductAlreadyFavorited")]
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

        [FlowStep(order: 6, bpmnId: "SaveFavorite")]
        public async Task Save(Favorite favorite, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            await favoriteRepository.AddAsync(favorite, cancellationToken);
        }

        [FlowStep(order: 7, bpmnId: "MapFavoriteResponse")]
        public FavoriteResponseDto MapToResponse(Favorite favorite)
        {
            return MappingConfig.MapToResponse(favorite);
        }
    }
}

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

    internal sealed class GetFavoritesByClientIdDescriptor : FlowDescriberBase<GetFavoritesByClientId>
    {
        [FlowStep(order: 1, bpmnId: "ValidateClientId")]
        public async Task<ValidationResult> ValidateClientId(Guid clientId, IValidationPolicy<Guid> emptyGuidValidationPolicy)
        {
            return await emptyGuidValidationPolicy.Validate(clientId);
        }

        [FlowStep(order: 2, bpmnId: "IsClientIdValid")]
        public void ThrowValidationExceptionIfClientIdInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 3, bpmnId: "LoadFavoritesByClientId")]
        public async Task<IReadOnlyList<Favorite>> LoadFavorites(Guid clientId, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            return await favoriteRepository.GetByClientIdAsync(clientId, cancellationToken);
        }

        [FlowStep(order: 4, bpmnId: "MapFavoritesResponse")]
        public IReadOnlyList<FavoriteResponseDto> MapToResponse(IReadOnlyList<Favorite> favorites)
        {
            return favorites.Select(MappingConfig.MapToResponse).ToList();
        }
    }
}

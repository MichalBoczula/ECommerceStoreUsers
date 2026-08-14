using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Favorites
{
    internal sealed record ClearClientFavorites;

    internal sealed class ClearClientFavoritesDescriptor : FlowDescriberBase<ClearClientFavorites>
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

        [FlowStep(order: 3, bpmnId: "ClearAllFavorites")]
        public async Task ClearAll(Guid clientId, IFavoriteRepository favoriteRepository, CancellationToken cancellationToken)
        {
            await favoriteRepository.DeleteAllByClientIdAsync(clientId, cancellationToken);
        }
    }
}

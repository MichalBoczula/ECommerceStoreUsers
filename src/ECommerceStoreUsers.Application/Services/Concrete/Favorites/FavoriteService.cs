using ECommerceStoreUsers.Application.Common.RequestsDto.Favorites;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Favorites;
using ECommerceStoreUsers.Application.Descriptors.Favorites;
using ECommerceStoreUsers.Application.Services.Abstract.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using Microsoft.Extensions.Logging;

namespace ECommerceStoreUsers.Application.Services.Concrete.Favorites
{
    internal sealed class FavoriteService(
     IFavoriteRepository _favoriteRepository,
     IValidationPolicy<Favorite> _favoriteValidationPolicy,
     IValidationPolicy<Guid> _emptyGuidValidationPolicy,
     ILogger<FavoriteService> _logger) : IFavoriteService
    {
        public async Task<IReadOnlyList<FavoriteResponseDto>> GetFavoritesByClientId(Guid clientId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating get favorites flow for ClientId: {ClientId}", clientId);

            var descriptor = new GetFavoritesByClientIdDescriptor();

            var validationResult = await descriptor.ValidateClientId(clientId, _emptyGuidValidationPolicy);
            descriptor.ThrowValidationExceptionIfClientIdInvalid(validationResult);

            var favorites = await descriptor.LoadFavorites(clientId, _favoriteRepository, cancellationToken);

            var response = descriptor.MapToResponse(favorites);

            _logger.LogInformation("Successfully retrieved {Count} favorites for ClientId: {ClientId}", response.Count, clientId);

            return response;
        }

        public async Task<FavoriteResponseDto> AddProductToFavorites(Guid clientId, AddFavoriteRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating add favorite flow for ClientId: {ClientId}, ProductId: {ProductId}", clientId, request.ProductId);

            var descriptor = new AddProductToFavoritesDescriptor();

            var favorite = descriptor.MapToDomain(clientId, request);

            var validationResult = await descriptor.ValidateFavorite(favorite, _favoriteValidationPolicy);
            descriptor.ThrowValidationExceptionIfFavoriteInvalid(validationResult);

            var exists = await descriptor.CheckExists(clientId, request.ProductId, _favoriteRepository, cancellationToken);
            descriptor.ThrowAlreadyExistsExceptionIfFavorited(clientId, request.ProductId, exists);

            await descriptor.Save(favorite, _favoriteRepository, cancellationToken);

            var response = descriptor.MapToResponse(favorite);

            _logger.LogInformation("Successfully added favorite. FavoriteId: {FavoriteId} for ClientId: {ClientId}", response.Id, clientId);

            return response;
        }

        public async Task RemoveProductFromFavorites(Guid clientId, Guid productId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating remove favorite flow for ClientId: {ClientId}, ProductId: {ProductId}", clientId, productId);

            var descriptor = new RemoveProductFromFavoritesDescriptor();

            var validationResult = await descriptor.ValidateIdentifiers(clientId, productId, _emptyGuidValidationPolicy);
            descriptor.ThrowValidationExceptionIfIdentifiersInvalid(validationResult);

            var favorite = await descriptor.LoadFavorite(clientId, productId, _favoriteRepository, cancellationToken);
            descriptor.ThrowNotFoundExceptionIfFavoriteMissing(clientId, productId, favorite);

            await descriptor.Delete(clientId, productId, _favoriteRepository, cancellationToken);

            _logger.LogInformation("Successfully removed favorite for ClientId: {ClientId}, ProductId: {ProductId}", clientId, productId);
        }

        public async Task ClearClientFavorites(Guid clientId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating clear all favorites flow for ClientId: {ClientId}", clientId);

            var descriptor = new ClearClientFavoritesDescriptor();

            var validationResult = await descriptor.ValidateClientId(clientId, _emptyGuidValidationPolicy);
            descriptor.ThrowValidationExceptionIfClientIdInvalid(validationResult);

            await descriptor.ClearAll(clientId, _favoriteRepository, cancellationToken);

            _logger.LogInformation("Successfully cleared all favorites for ClientId: {ClientId}", clientId);
        }
    }
}

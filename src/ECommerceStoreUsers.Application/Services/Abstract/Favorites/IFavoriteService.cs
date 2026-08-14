using ECommerceStoreUsers.Application.Common.RequestsDto.Favorites;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Favorites;

namespace ECommerceStoreUsers.Application.Services.Abstract.Favorites
{
    public interface IFavoriteService
    {
        Task<IReadOnlyList<FavoriteResponseDto>> GetFavoritesByClientId(Guid clientId, CancellationToken cancellationToken);
        Task<FavoriteResponseDto> AddProductToFavorites(Guid clientId, AddFavoriteRequestDto request, CancellationToken cancellationToken);
        Task RemoveProductFromFavorites(Guid clientId, Guid productId, CancellationToken cancellationToken);
        Task ClearClientFavorites(Guid clientId, CancellationToken cancellationToken);
    }
}

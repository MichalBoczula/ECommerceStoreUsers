using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Infrastructure.Persistance.Favorites;

namespace ECommerceStoreUsers.Infrastructure.Mapping
{
    internal static class FavoriteMapping
    {
        internal static FavoriteDocument MapToDocument(Favorite favorite)
        {
            return new FavoriteDocument
            {
                Id = favorite.Id,
                ClientId = favorite.ClientId,
                ProductId = favorite.ProductId,
                AddedAt = favorite.AddedAt
            };
        }

        internal static Favorite MapToDomain(FavoriteDocument favoriteDocument)
        {
            return Favorite.Rehydrate(
                favoriteDocument.Id,
                favoriteDocument.ClientId,
                favoriteDocument.ProductId,
                favoriteDocument.AddedAt
            );
        }
    }
}
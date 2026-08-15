using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.AggregatesModel
{
    public class FavoritesTests
    {
        [Fact]
        public void Constructor_ShouldInitializeCoreProperties()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Act
            var favorite = new Favorite(clientId, productId);

            // Assert
            favorite.Id.ShouldNotBe(Guid.Empty);
            favorite.ClientId.ShouldBe(clientId);
            favorite.ProductId.ShouldBe(productId);
            favorite.AddedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void Rehydrate_ShouldRebuildFavoriteWithGivenValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var addedAt = DateTime.UtcNow.AddDays(-2);

            // Act
            var favorite = Favorite.Rehydrate(id, clientId, productId, addedAt);

            // Assert
            favorite.Id.ShouldBe(id);
            favorite.ClientId.ShouldBe(clientId);
            favorite.ProductId.ShouldBe(productId);
            favorite.AddedAt.ShouldBe(addedAt);
        }
    }
}

using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Persistance.Favorites;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Mapping;

public sealed class FavoriteMappingTests
{
    [Fact]
    public void MapToDocument_ShouldMapAllFavoriteFields()
    {
        var favorite = CreateFavorite();

        var document = FavoriteMapping.MapToDocument(favorite);

        document.Id.ShouldBe(favorite.Id);
        document.ClientId.ShouldBe(favorite.ClientId);
        document.ProductId.ShouldBe(favorite.ProductId);
        document.AddedAt.ShouldBe(favorite.AddedAt);
    }

    [Fact]
    public void MapToDomain_ShouldMapAllFavoriteFields()
    {
        var document = CreateFavoriteDocument();

        var favorite = FavoriteMapping.MapToDomain(document);

        favorite.Id.ShouldBe(document.Id);
        favorite.ClientId.ShouldBe(document.ClientId);
        favorite.ProductId.ShouldBe(document.ProductId);
        favorite.AddedAt.ShouldBe(document.AddedAt);
    }

    private static Favorite CreateFavorite() => Favorite.Rehydrate(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));

    private static FavoriteDocument CreateFavoriteDocument() => new()
    {
        Id = Guid.NewGuid(),
        ClientId = Guid.NewGuid(),
        ProductId = Guid.NewGuid(),
        AddedAt = new DateTime(2026, 4, 20, 10, 30, 0, DateTimeKind.Utc)
    };
}

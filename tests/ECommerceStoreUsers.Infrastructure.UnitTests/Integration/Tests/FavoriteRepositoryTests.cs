using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Tests
{
    public sealed class FavoriteRepositoryTests : IClassFixture<MongoDbTestFixture>
    {
        private readonly MongoDbTestFixture _fixture;

        public FavoriteRepositoryTests(MongoDbTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AddAsync_ShouldSaveFavoriteToDatabase()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();
            var favorite = new Favorite(Guid.NewGuid(), Guid.NewGuid());

            await repository.AddAsync(favorite, CancellationToken.None);

            var result = await repository.GetByClientAndProductIdAsync(
                favorite.ClientId, favorite.ProductId, CancellationToken.None);

            result.ShouldNotBeNull();
            result.Id.ShouldBe(favorite.Id);
            result.ClientId.ShouldBe(favorite.ClientId);
            result.ProductId.ShouldBe(favorite.ProductId);
            result.AddedAt.ShouldBe(favorite.AddedAt);
        }

        [Fact]
        public async Task GetByClientIdAsync_ShouldReturnOnlyClientFavoritesOrderedByNewestFirst()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();
            var clientId = Guid.NewGuid();
            var older = CreateFavorite(clientId, DateTime.UtcNow.AddDays(-2));
            var newer = CreateFavorite(clientId, DateTime.UtcNow.AddDays(-1));
            var otherClientFavorite = CreateFavorite(Guid.NewGuid(), DateTime.UtcNow);
            await repository.AddAsync(older, CancellationToken.None);
            await repository.AddAsync(otherClientFavorite, CancellationToken.None);
            await repository.AddAsync(newer, CancellationToken.None);

            var results = await repository.GetByClientIdAsync(clientId, CancellationToken.None);

            results.Count.ShouldBe(2);
            results.Select(x => x.Id).ShouldBe(new[] { newer.Id, older.Id });
        }

        [Fact]
        public async Task GetByClientIdAsync_ShouldReturnEmptyList_WhenClientHasNoFavorites()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();

            var results = await repository.GetByClientIdAsync(Guid.NewGuid(), CancellationToken.None);

            results.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetByClientAndProductIdAsync_ShouldReturnMatchingFavorite()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();
            var favorite = new Favorite(Guid.NewGuid(), Guid.NewGuid());
            await repository.AddAsync(favorite, CancellationToken.None);

            var result = await repository.GetByClientAndProductIdAsync(
                favorite.ClientId, favorite.ProductId, CancellationToken.None);

            result.ShouldNotBeNull();
            result.Id.ShouldBe(favorite.Id);
        }

        [Fact]
        public async Task GetByClientAndProductIdAsync_ShouldReturnNull_WhenFavoriteDoesNotExist()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();

            var result = await repository.GetByClientAndProductIdAsync(
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenFavoriteExists()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();
            var favorite = new Favorite(Guid.NewGuid(), Guid.NewGuid());
            await repository.AddAsync(favorite, CancellationToken.None);

            var result = await repository.ExistsAsync(
                favorite.ClientId, favorite.ProductId, CancellationToken.None);

            result.ShouldBeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenFavoriteDoesNotExist()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();

            var result = await repository.ExistsAsync(
                Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

            result.ShouldBeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteOnlyMatchingFavorite()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();
            var clientId = Guid.NewGuid();
            var favoriteToDelete = new Favorite(clientId, Guid.NewGuid());
            var favoriteToKeep = new Favorite(clientId, Guid.NewGuid());
            await repository.AddAsync(favoriteToDelete, CancellationToken.None);
            await repository.AddAsync(favoriteToKeep, CancellationToken.None);

            await repository.DeleteAsync(clientId, favoriteToDelete.ProductId, CancellationToken.None);

            (await repository.ExistsAsync(clientId, favoriteToDelete.ProductId, CancellationToken.None)).ShouldBeFalse();
            (await repository.ExistsAsync(clientId, favoriteToKeep.ProductId, CancellationToken.None)).ShouldBeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotRemoveFavorites_WhenMatchDoesNotExist()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();
            var favorite = new Favorite(Guid.NewGuid(), Guid.NewGuid());
            await repository.AddAsync(favorite, CancellationToken.None);

            await repository.DeleteAsync(favorite.ClientId, Guid.NewGuid(), CancellationToken.None);

            (await repository.ExistsAsync(favorite.ClientId, favorite.ProductId, CancellationToken.None)).ShouldBeTrue();
        }

        [Fact]
        public async Task DeleteAllByClientIdAsync_ShouldDeleteOnlySpecifiedClientFavorites()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();
            var clientId = Guid.NewGuid();
            var first = new Favorite(clientId, Guid.NewGuid());
            var second = new Favorite(clientId, Guid.NewGuid());
            var otherClientFavorite = new Favorite(Guid.NewGuid(), Guid.NewGuid());
            await repository.AddAsync(first, CancellationToken.None);
            await repository.AddAsync(second, CancellationToken.None);
            await repository.AddAsync(otherClientFavorite, CancellationToken.None);

            await repository.DeleteAllByClientIdAsync(clientId, CancellationToken.None);

            (await repository.GetByClientIdAsync(clientId, CancellationToken.None)).ShouldBeEmpty();
            (await repository.ExistsAsync(
                otherClientFavorite.ClientId, otherClientFavorite.ProductId, CancellationToken.None)).ShouldBeTrue();
        }

        private ServiceProvider CreateServiceProvider()
        {
            return TestServiceProviderFactory.Create(
                _fixture.ConnectionString, $"favorite-tests-{Guid.NewGuid():N}");
        }

        private static Favorite CreateFavorite(Guid clientId, DateTime addedAt)
        {
            return Favorite.Rehydrate(Guid.NewGuid(), clientId, Guid.NewGuid(), addedAt);
        }
    }
}

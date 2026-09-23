using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Infrastructure;
using ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
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
            result.AddedAt.ShouldBe(favorite.AddedAt, TimeSpan.FromMilliseconds(1));
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

            var deleted = await repository.DeleteAsync(clientId, favoriteToDelete.ProductId, CancellationToken.None);

            deleted.ShouldBeTrue();
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

            var deleted = await repository.DeleteAsync(favorite.ClientId, Guid.NewGuid(), CancellationToken.None);

            deleted.ShouldBeFalse();
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

        [Fact]
        public async Task ConcurrentAddsOfSamePair_ShouldPersistOneAndReportOneConflict()
        {
            await using var serviceProvider = CreateServiceProvider();
            await serviceProvider.InitializeInfrastructureAsync();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();
            var clientId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            async Task<Exception?> TryAddAsync()
            {
                try
                {
                    await repository.AddAsync(new Favorite(clientId, productId), CancellationToken.None);
                    return null;
                }
                catch (Exception exception)
                {
                    return exception;
                }
            }

            var outcomes = await Task.WhenAll(TryAddAsync(), TryAddAsync());

            outcomes.Count(x => x is null).ShouldBe(1);
            outcomes.Count(x => x is ResourceAlreadyExistsException).ShouldBe(1);
            (await repository.GetByClientIdAsync(clientId, CancellationToken.None)).Count.ShouldBe(1);
        }

        [Fact]
        public async Task DuplicateIdForDifferentPair_ShouldRemainMongoWriteException()
        {
            await using var serviceProvider = CreateServiceProvider();
            await serviceProvider.InitializeInfrastructureAsync();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();
            var first = new Favorite(Guid.NewGuid(), Guid.NewGuid());
            await repository.AddAsync(first, CancellationToken.None);
            var second = Favorite.Rehydrate(first.Id, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

            var error = await Should.ThrowAsync<MongoWriteException>(() =>
                repository.AddAsync(second, CancellationToken.None));

            error.WriteError.Code.ShouldBe(11000);
            (await repository.GetByClientIdAsync(second.ClientId, CancellationToken.None)).ShouldBeEmpty();
        }

        [Fact]
        public async Task ConcurrentDeletes_ShouldReportExactlyOneDeletedFavorite()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();
            var favorite = new Favorite(Guid.NewGuid(), Guid.NewGuid());
            await repository.AddAsync(favorite, CancellationToken.None);

            var outcomes = await Task.WhenAll(
                repository.DeleteAsync(favorite.ClientId, favorite.ProductId, CancellationToken.None),
                repository.DeleteAsync(favorite.ClientId, favorite.ProductId, CancellationToken.None));

            outcomes.Count(deleted => deleted).ShouldBe(1);
            outcomes.Count(deleted => !deleted).ShouldBe(1);
            (await repository.GetByClientIdAsync(favorite.ClientId, CancellationToken.None)).ShouldBeEmpty();
        }

        [Fact]
        public async Task ClearEmptyFavorites_ShouldRemainSuccessfulNoOp()
        {
            await using var serviceProvider = CreateServiceProvider();
            var repository = serviceProvider.GetRequiredService<IFavoriteRepository>();
            var clientId = Guid.NewGuid();

            await repository.DeleteAllByClientIdAsync(clientId, CancellationToken.None);

            (await repository.GetByClientIdAsync(clientId, CancellationToken.None)).ShouldBeEmpty();
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

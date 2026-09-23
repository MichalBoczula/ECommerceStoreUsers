using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reqnroll;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Favorites;

[Binding]
public sealed class FavoriteRacesStepDefinitions(ScenarioApiContext context) : IDisposable
{
    private WebApplicationFactory<ECommerceStoreUsers.API.Program>? _factory;

    [Given("another request inserts the favorite after the existence check")]
    public void GivenAnotherRequestInsertsTheFavoriteAfterTheExistenceCheck()
    {
        UseRacingRepository(FavoriteRace.Insert);
    }

    [Given("another request deletes the favorite after it is loaded")]
    public void GivenAnotherRequestDeletesTheFavoriteAfterItIsLoaded()
    {
        UseRacingRepository(FavoriteRace.Delete);
    }

    [Then("exactly one matching favorite remains")]
    public async Task ThenExactlyOneMatchingFavoriteRemains()
    {
        var favorites = await LoadFavorites();
        favorites.Count.ShouldBe(1);
        favorites[0].ProductId.ShouldBe(context.FavoriteProductId);
    }

    [Then("no matching favorite remains")]
    public async Task ThenNoMatchingFavoriteRemains()
    {
        (await LoadFavorites()).ShouldBeEmpty();
    }

    private void UseRacingRepository(FavoriteRace race)
    {
        _factory = context.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IFavoriteRepository>();
            services.AddScoped<IFavoriteRepository>(provider =>
                new RacingFavoriteRepository(provider.GetRequiredService<MongoDbContext>(), race));
        }));

        context.HttpClient.Dispose();
        context.HttpClient = _factory.CreateClient();
    }

    private async Task<List<FavoriteResponseDto>> LoadFavorites()
    {
        using var response = await context.HttpClient.GetAsync($"/favorites/clients/{context.FavoriteClientId}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<FavoriteResponseDto>>(context.JsonOptions))!;
    }

    public void Dispose() => _factory?.Dispose();

    private enum FavoriteRace { Insert, Delete }

    private sealed class RacingFavoriteRepository(MongoDbContext mongoContext, FavoriteRace race) : IFavoriteRepository
    {
        private readonly FavoriteRepository _inner = new(mongoContext);
        private bool _triggered;

        public Task<IReadOnlyList<Favorite>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
            => _inner.GetByClientIdAsync(clientId, cancellationToken);

        public async Task<Favorite?> GetByClientAndProductIdAsync(
            Guid clientId, Guid productId, CancellationToken cancellationToken = default)
        {
            var favorite = await _inner.GetByClientAndProductIdAsync(clientId, productId, cancellationToken);
            if (race == FavoriteRace.Delete && favorite is not null && !_triggered)
            {
                _triggered = true;
                if (!await _inner.DeleteAsync(clientId, productId, cancellationToken))
                    throw new InvalidOperationException("Failed to arrange the concurrent favorite delete.");
            }

            return favorite;
        }

        public async Task<bool> ExistsAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default)
        {
            var exists = await _inner.ExistsAsync(clientId, productId, cancellationToken);
            if (race == FavoriteRace.Insert && !exists && !_triggered)
            {
                _triggered = true;
                await _inner.AddAsync(new Favorite(clientId, productId), cancellationToken);
            }

            return exists;
        }

        public Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
            => _inner.AddAsync(favorite, cancellationToken);

        public Task<bool> DeleteAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default)
            => _inner.DeleteAsync(clientId, productId, cancellationToken);

        public Task DeleteAllByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
            => _inner.DeleteAllByClientIdAsync(clientId, cancellationToken);
    }
}

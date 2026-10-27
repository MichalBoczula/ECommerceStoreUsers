using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using Reqnroll;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Favorites;

[Binding]
public sealed class FavoriteRacesStepDefinitions(ScenarioApiContext context, AtomicityState atomicity, FavoriteSuccessState success) : IDisposable
{
    private WebApplicationFactory<ECommerceStoreUsers.API.Program>? _factory;
    private FavoriteRace _race;

    [Given("another request inserts the favorite after the existence check")]
    public Task GivenAnotherRequestInsertsTheFavoriteAfterTheExistenceCheck() => UseRacingRepository(FavoriteRace.Insert);

    [Given("another request deletes the favorite after it is loaded")]
    public Task GivenAnotherRequestDeletesTheFavoriteAfterItIsLoaded() => UseRacingRepository(FavoriteRace.Delete);

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

    private async Task UseRacingRepository(FavoriteRace race)
    {
        _race = race;
        await success.SeedAsync(context.FavoriteClientId, Guid.Parse("11111111-1111-1111-1111-111111111111"));
        await success.SeedAsync(Guid.Parse("99999999-9999-9999-9999-999999999999"), context.FavoriteProductId);
        await atomicity.BeginAsync();
        _factory = context.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IFavoriteRepository>();
            services.AddScoped<IFavoriteRepository>(provider =>
                new RacingFavoriteRepository(provider.GetRequiredService<MongoDbContext>(), race, atomicity));
        }));

        context.HttpClient.Dispose();
        context.HttpClient = context.CreateClient(_factory);
    }

    private async Task<List<FavoriteResponseDto>> LoadFavorites()
    {
        using var response = await context.HttpClient.GetAsync($"/favorites/clients/{context.FavoriteClientId}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<FavoriteResponseDto>>(context.JsonOptions))!
            .Where(item => item.ProductId == context.FavoriteProductId).ToList();
    }

    [Then("the favorite race problem is complete")]
    public Task ThenProblem()
    {
        var identifier = $"{context.FavoriteClientId}:{context.FavoriteProductId}";
        return _race == FavoriteRace.Insert
            ? atomicity.ProblemAsync(409, "resource_conflict", "Conflict.",
                $"Resource Favorite identified by id {identifier} already exists in db. Error in action AddProductToFavorites.")
            : atomicity.ProblemAsync(404, "resource_not_found", "Resource not found.",
                $"Resource Favorite identified by id {identifier} cannot be found in database during action RemoveProductFromFavorites.");
    }

    public void Dispose() => _factory?.Dispose();

    private enum FavoriteRace { Insert, Delete }

    private sealed class RacingFavoriteRepository(MongoDbContext mongoContext, FavoriteRace race, AtomicityState atomicity) : IFavoriteRepository
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
                await atomicity.CommitDeleteAsync("favorites", new BsonBinaryData(favorite.Id, GuidRepresentation.Standard));
            }

            return favorite;
        }

        public async Task<bool> ExistsAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default)
        {
            var exists = await _inner.ExistsAsync(clientId, productId, cancellationToken);
            if (race == FavoriteRace.Insert && !exists && !_triggered)
            {
                _triggered = true;
                var winner = new Favorite(clientId, productId);
                await _inner.AddAsync(winner, cancellationToken);
                await atomicity.CommitFavoriteAsync(FavoriteMapping.MapToDocument(winner).ToBsonDocument());
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


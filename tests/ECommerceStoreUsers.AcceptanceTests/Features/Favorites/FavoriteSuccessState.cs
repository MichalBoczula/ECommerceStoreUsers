using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Favorites;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Favorites;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Persistence.Favorites;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Favorites;

/// <summary>Scenario-local complete collection evidence; Favorites have no history.</summary>
public sealed class FavoriteSuccessState(ScenarioApiContext context)
{
    private FavoriteDocument[]? _before;
    private FavoriteDocument[]? _after;
    private DateTime _started;
    private DateTime _finished;
    private bool _observe;

    internal void Observe() => _observe = true;

    internal async Task ExecuteAsync(Func<Task<HttpResponseMessage>> send)
    {
        if (_observe)
        {
            _before = await LoadAsync();
            Attach("Favorite database before", _before);
            _started = DateTime.UtcNow;
        }
        context.Response = await send();
        if (_observe)
        {
            _finished = DateTime.UtcNow;
            _after = await LoadAsync();
            // Keep the actual resulting collection even when a response assertion fails.
            Attach("Favorite database after", _after);
        }
    }

    internal async Task SeedAsync(Guid clientId, Guid productId)
    {
        var before = await LoadAsync();
        var request = new AddFavoriteRequestDto { ProductId = productId };
        var started = DateTime.UtcNow;
        using var response = await context.HttpClient.PostAsJsonAsync(
            $"/favorites/clients/{clientId}", request, context.JsonOptions);
        var finished = DateTime.UtcNow;
        var after = await LoadAsync();
        Attach("Favorite HTTP setup", new { clientId, request, before, after, started, finished });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        var favorite = JsonSerializer.Deserialize<FavoriteResponseDto>(
            await response.Content.ReadAsStringAsync(), context.JsonOptions);
        favorite.ShouldNotBeNull();
        favorite.ClientId.ShouldBe(clientId);
        favorite.ProductId.ShouldBe(productId);
        AssertFields(favorite);
        InWindow(favorite.AddedAt, started, finished);
        after.Length.ShouldBe(before.Length + 1);
        var inserted = after.Single(item => item.ClientId == clientId && item.ProductId == productId);
        AssertEqual(inserted, Document(favorite));
        AssertEqual(after.Where(item => item.Id != inserted.Id).ToArray(), before);
    }

    internal async Task SeedDatedAsync(Table table)
    {
        // Explicit dates make the real MongoDB sort test deterministic without sleeps.
        var before = await LoadAsync();
        var entries = table.Rows.Select(row => new FavoriteDocument
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.Parse(row["ClientId"]),
            ProductId = Guid.Parse(row["ProductId"]),
            AddedAt = DateTime.Parse(row["AddedAt"], CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal)
        }).ToArray();
        entries.ShouldNotBeEmpty();
        foreach (var entry in entries)
        {
            entry.ClientId.ShouldNotBe(Guid.Empty);
            entry.ProductId.ShouldNotBe(Guid.Empty);
            entry.AddedAt.Kind.ShouldBe(DateTimeKind.Utc);
            entry.AddedAt.ShouldNotBe(default);
        }
        await Mongo.Favorites.InsertManyAsync(entries);
        var after = await LoadAsync();
        Attach("Favorite dated MongoDB setup", new { entries, before, after });
        AssertEqual(after, before.Concat(entries).OrderBy(item => item.Id).ToArray());
    }

    internal async Task ExpectAddedAsync(Table table)
    {
        var expected = Values(table);
        Attach("Favorite response expectations", expected);
        RequireResponse(HttpStatusCode.OK, "application/json");
        var favorite = JsonSerializer.Deserialize<FavoriteResponseDto>(
            await context.Response!.Content.ReadAsStringAsync(), context.JsonOptions);
        favorite.ShouldNotBeNull();
        favorite.ClientId.ShouldBe(Guid.Parse(Required(expected, "ClientId")));
        favorite.ProductId.ShouldBe(Guid.Parse(Required(expected, "ProductId")));
        Required(expected, "Id").ShouldBe("generated");
        Required(expected, "AddedAt").ShouldBe("generated");
        AssertFields(favorite);
        InWindow(favorite.AddedAt, _started, _finished);
        _after.ShouldNotBeNull();
        AssertEqual(_after.Single(item => item.Id == favorite.Id), Document(favorite));
    }

    internal async Task ExpectListAsync(Table table)
    {
        Attach("Favorite list expectations", table.Rows.Select(row => new
        {
            clientId = row["ClientId"],
            productId = row["ProductId"]
        }).ToArray());
        RequireResponse(HttpStatusCode.OK, "application/json");
        var favorites = JsonSerializer.Deserialize<List<FavoriteResponseDto>>(
            await context.Response!.Content.ReadAsStringAsync(), context.JsonOptions);
        favorites.ShouldNotBeNull();
        AssertPairs(favorites.Select(Document).ToArray(), table);
        _before.ShouldNotBeNull();
        foreach (var favorite in favorites)
        {
            AssertFields(favorite);
            favorite.ClientId.ShouldBe(context.FavoriteClientId);
            AssertEqual(Document(favorite), _before.Single(item => item.Id == favorite.Id));
        }
        // Equal timestamps have no promised secondary order.
        favorites.Select(item => item.AddedAt).ShouldBe(favorites.Select(item => item.AddedAt).OrderByDescending(value => value));
        Attach("Favorite list verified", new { favorites, ordering = "AddedAt descending; ties unspecified" });
    }

    internal async Task ExpectEmptyListAsync()
    {
        RequireResponse(HttpStatusCode.OK, "application/json");
        var favorites = JsonSerializer.Deserialize<List<FavoriteResponseDto>>(
            await context.Response!.Content.ReadAsStringAsync(), context.JsonOptions);
        favorites.ShouldNotBeNull();
        favorites.ShouldBeEmpty();
        Attach("Favorite list verified", new { favorites, expectedCount = 0 });
    }

    internal async Task ExpectNoContentAsync()
    {
        RequireResponse(HttpStatusCode.NoContent);
        (await context.Response!.Content.ReadAsByteArrayAsync()).ShouldBeEmpty();
        context.Response.Content.Headers.ContentType.ShouldBeNull();
        Attach("Favorite response verified", new { statusCode = 204, byteLength = 0, contentType = (string?)null });
    }

    internal void ExpectCollection(Table table)
    {
        _after.ShouldNotBeNull();
        Attach("Favorite collection expectations", table.Rows.Select(row => new
        {
            clientId = row["ClientId"],
            productId = row["ProductId"]
        }).ToArray());
        AssertPairs(_after, table);
    }

    internal void ExpectEmptyCollection()
    {
        _after.ShouldNotBeNull();
        _after.ShouldBeEmpty();
        Attach("Favorite collection expectations", Array.Empty<FavoriteDocument>());
    }

    internal void ExpectEffect(Table table)
    {
        var expected = Values(table);
        Attach("Favorite persistence expectations", expected);
        _before.ShouldNotBeNull();
        _after.ShouldNotBeNull();
        _before.Length.ShouldBe(int.Parse(Required(expected, "BeforeCount"), CultureInfo.InvariantCulture));
        _after.Length.ShouldBe(int.Parse(Required(expected, "AfterCount"), CultureInfo.InvariantCulture));
        var effect = Required(expected, "WriteEffect");
        var clientId = context.FavoriteClientId;
        var productId = context.FavoriteProductId;
        if (effect == "Insert")
        {
            _before.Any(item => item.ClientId == clientId && item.ProductId == productId).ShouldBeFalse();
            var inserted = _after.Single(item => item.ClientId == clientId && item.ProductId == productId);
            inserted.Id.ShouldNotBe(Guid.Empty);
            _before.Any(item => item.Id == inserted.Id).ShouldBeFalse();
            InWindow(inserted.AddedAt, _started, _finished);
            AssertEqual(_after.Where(item => item.Id != inserted.Id).ToArray(), _before);
        }
        else if (effect == "Remove")
        {
            _before.Count(item => item.ClientId == clientId && item.ProductId == productId).ShouldBe(1);
            AssertEqual(_after, _before.Where(item => item.ClientId != clientId || item.ProductId != productId).ToArray());
        }
        else if (effect == "Clear")
        {
            _before.Any(item => item.ClientId == clientId).ShouldBeTrue();
            AssertEqual(_after, _before.Where(item => item.ClientId != clientId).ToArray());
        }
        else
        {
            effect.ShouldBe("None");
            AssertEqual(_after, _before);
        }
        Attach("Favorite persistence verified", new
        {
            writeEffect = effect,
            clientId,
            productId,
            beforeCount = _before.Length,
            afterCount = _after.Length,
            unrelatedEntriesUnchanged = true,
            requestStartedAt = _started,
            requestCompletedAt = _finished
        });
    }

    private MongoDbContext Mongo => context.Factory.Services.GetRequiredService<MongoDbContext>();
    private async Task<FavoriteDocument[]> LoadAsync()
        => (await Mongo.Favorites.Find(FilterDefinition<FavoriteDocument>.Empty).ToListAsync()).OrderBy(item => item.Id).ToArray();
    private void Attach(string name, object value) => context.RecordEvidence(name, value);
    private static Dictionary<string, string> Values(Table table)
        => table.Rows.ToDictionary(row => row["Field"], row => row["Value"], StringComparer.Ordinal);
    private static string Required(IReadOnlyDictionary<string, string> values, string field)
        => values.TryGetValue(field, out var value) ? value : throw new InvalidOperationException($"Missing explicit Favorite expectation: {field}");
    private void RequireResponse(HttpStatusCode status, string? mediaType = null)
    {
        context.Response.ShouldNotBeNull();
        context.Response.StatusCode.ShouldBe(status);
        if (mediaType is not null)
            context.Response.Content.Headers.ContentType?.MediaType.ShouldBe(mediaType);
    }
    private static void AssertFields(FavoriteResponseDto favorite)
    {
        favorite.Id.ShouldNotBe(Guid.Empty);
        favorite.AddedAt.ShouldNotBe(default);
        favorite.AddedAt.Kind.ShouldBe(DateTimeKind.Utc);
    }
    private static FavoriteDocument Document(FavoriteResponseDto favorite) => new()
    {
        Id = favorite.Id,
        ClientId = favorite.ClientId,
        ProductId = favorite.ProductId,
        AddedAt = DateTimeOffset.FromUnixTimeMilliseconds(new DateTimeOffset(favorite.AddedAt).ToUnixTimeMilliseconds()).UtcDateTime
    };
    private static void InWindow(DateTime value, DateTime started, DateTime finished)
    {
        value.Kind.ShouldBe(DateTimeKind.Utc);
        var actual = new DateTimeOffset(value).ToUnixTimeMilliseconds();
        actual.ShouldBeGreaterThanOrEqualTo(new DateTimeOffset(started).ToUnixTimeMilliseconds());
        actual.ShouldBeLessThanOrEqualTo(new DateTimeOffset(finished).ToUnixTimeMilliseconds());
    }
    private static void AssertPairs(FavoriteDocument[] actual, Table table)
    {
        var expected = table.Rows.Select(row => (ClientId: Guid.Parse(row["ClientId"]), ProductId: Guid.Parse(row["ProductId"])))
            .OrderBy(item => item.ClientId).ThenBy(item => item.ProductId).ToArray();
        actual.Select(item => (item.ClientId, item.ProductId)).OrderBy(item => item.ClientId).ThenBy(item => item.ProductId).ShouldBe(expected);
    }
    private static void AssertEqual(object actual, object expected)
        => JsonElement.DeepEquals(JsonSerializer.SerializeToElement(actual), JsonSerializer.SerializeToElement(expected))
            .ShouldBeTrue("All Favorite fields, including retained IDs and timestamps, must match.");
}

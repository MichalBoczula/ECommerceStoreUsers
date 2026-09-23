using MongoDB.Bson;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class AcceptanceIsolationStepDefinitions(ScenarioApiContext apiContext)
{
    private const string Marker = "REF05 isolation marker";

    [Given("this Users scenario has no REF-05 customer marker in current or history")]
    [Then("this Users scenario has no REF-05 customer marker in current or history")]
    public async Task TheScenarioHasNoMarkers()
    {
        var database = AcceptanceMongoDb.GetDatabase(apiContext.DatabaseName);
        (await CountMarkers(database, "customers")).ShouldBe(0);
        (await CountMarkers(database, "customers-history")).ShouldBe(0);
    }

    [When("I store a REF-05 customer marker in current and history")]
    public async Task StoreScenarioMarkers()
    {
        var database = AcceptanceMongoDb.GetDatabase(apiContext.DatabaseName);
        var marker = new BsonDocument("ref05Marker", Marker);
        await database.GetCollection<BsonDocument>("customers").InsertOneAsync(marker);
        await database.GetCollection<BsonDocument>("customers-history")
            .InsertOneAsync(new BsonDocument("ref05Marker", Marker));
    }

    [Then("this Users scenario contains only its own REF-05 markers")]
    public async Task TheScenarioHasItsOwnMarkers()
    {
        var database = AcceptanceMongoDb.GetDatabase(apiContext.DatabaseName);
        (await CountMarkers(database, "customers")).ShouldBe(1);
        (await CountMarkers(database, "customers-history")).ShouldBe(1);
    }

    private static Task<long> CountMarkers(IMongoDatabase database, string collection)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("ref05Marker", Marker);
        return database.GetCollection<BsonDocument>(collection).CountDocumentsAsync(filter);
    }
}

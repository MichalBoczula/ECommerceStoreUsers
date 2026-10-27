using MongoDB.Bson;
using MongoDB.Bson.IO;
using System.Text.Json;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class AcceptanceIsolationStepDefinitions(ScenarioApiContext apiContext, FrameworkFailureState state)
{
    private Dictionary<string, JsonElement[]>? _expected;
    private const string Marker = "REF05 isolation marker";

    [Given("this Users scenario has no REF-05 customer marker in current or history")]
    [Then("this Users scenario has no REF-05 customer marker in current or history")]
    public async Task TheScenarioHasNoMarkers()
    {
        var database = AcceptanceMongoDb.GetDatabase(apiContext.DatabaseName);
        (await CountMarkers(database, "customers")).ShouldBe(0);
        (await CountMarkers(database, "customers-history")).ShouldBe(0);
        apiContext.RecordEvidence("Technical isolation marker observation", new { database = apiContext.DatabaseName, snapshot = await state.SnapshotAsync(), markerCount = 0 });
    }

    [When("I store a REF-05 customer marker in current and history")]
    public async Task StoreScenarioMarkers()
    {
        var database = AcceptanceMongoDb.GetDatabase(apiContext.DatabaseName);
        var before = await state.SnapshotAsync();
        apiContext.RecordEvidence("Technical isolation before markers", before);
        before.Values.ShouldAllBe(documents => documents.Length == 0);
        _expected = new Dictionary<string, JsonElement[]>();
        foreach (var collection in before.Keys)
        {
            var marker = new BsonDocument
            {
                ["_id"] = new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard),
                ["ref05Marker"] = Marker,
                ["scenarioDatabase"] = apiContext.DatabaseName,
                ["collection"] = collection
            };
            await database.GetCollection<BsonDocument>(collection).InsertOneAsync(marker);
            _expected[collection] = [JsonSerializer.Deserialize<JsonElement>(marker.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson }))];
        }
        apiContext.RecordEvidence("Technical isolation expected markers", _expected);
    }

    [Then("this Users scenario contains only its own REF-05 markers")]
    public async Task TheScenarioHasItsOwnMarkers()
    {
        var database = AcceptanceMongoDb.GetDatabase(apiContext.DatabaseName);
        (await CountMarkers(database, "customers")).ShouldBe(1);
        (await CountMarkers(database, "customers-history")).ShouldBe(1);
        _expected.ShouldNotBeNull();
        var after = await state.SnapshotAsync();
        apiContext.RecordEvidence("Technical isolation after markers", after);
        JsonElement.DeepEquals(JsonSerializer.SerializeToElement(after), JsonSerializer.SerializeToElement(_expected)).ShouldBeTrue();
        apiContext.RecordEvidence("Technical isolation markers verified", new { database = apiContext.DatabaseName, collections = after.Keys, exactDocumentCount = 5, everyMarkerBelongsToThisDatabase = true });
    }

    private static Task<long> CountMarkers(IMongoDatabase database, string collection)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("ref05Marker", Marker);
        return database.GetCollection<BsonDocument>(collection).CountDocumentsAsync(filter);
    }
}


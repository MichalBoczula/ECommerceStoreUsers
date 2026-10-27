using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Domain.Validation.Common;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

/// <summary>Real MongoDB snapshots before interleaving, after the winner, and after the loser.</summary>
public sealed class AtomicityState(ScenarioApiContext context)
{
    private static readonly string[] Collections = ["customers", "customers-history", "admins", "admins-history", "favorites"];
    private static readonly JsonWriterSettings Canonical = new() { OutputMode = JsonOutputMode.CanonicalExtendedJson };
    private readonly FrameworkFailureState _problem = new(context);
    private Dictionary<string, BsonDocument[]>? _before;
    private Dictionary<string, BsonDocument[]>? _committed;
    private DateTime _started;
    private int _interleavings;
    private int _mongoFailures;
    private ResourceAlreadyExistsException? _createConflict;

    internal async Task BeginAsync()
    {
        _problem.UseEvidencePrefix("Atomicity");
        await _problem.BeginAsync();
        _before = await SnapshotAsync();
        _committed = null;
        _interleavings = 0;
        _mongoFailures = 0;
        _createConflict = null;
        _started = DateTime.UtcNow;
        Attach("Atomicity before interleaving", _before);
    }

    internal async Task CommitAggregateAsync(string collection, BsonDocument expected)
    {
        _before.ShouldNotBeNull();
        _committed = await SnapshotAsync();
        Attach("Atomicity committed winner", _committed);
        _interleavings++;
        _interleavings.ShouldBe(1);
        var id = expected["_id"];
        var previous = _before[collection].SingleOrDefault(item => item["_id"] == id);
        var version = previous is null ? 0 : previous["Version"].ToInt64() + 1;
        expected["Version"].ToInt64().ShouldBe(version);
        Equal(_committed[collection], _before[collection].Where(item => item["_id"] != id).Append(expected));
        var historyName = collection + "-history";
        var history = _committed[historyName];
        history.Length.ShouldBe(_before[historyName].Length + 1);
        foreach (var old in _before[historyName])
            Equal(history.Single(item => item["_id"] == old["_id"]), old);
        var added = history.Single(item => !_before[historyName].Any(old => old["_id"] == item["_id"]));
        var foreignKey = collection == "customers" ? "CustomerId" : "AdminId";
        added[foreignKey].ShouldBe(id);
        added["_id"].ShouldNotBe(new BsonBinaryData(Guid.Empty, GuidRepresentation.Standard));
        added["Action"].ToInt32().ShouldBe((int)(previous is null ? ActionType.Insert : ActionType.Update));
        var changedAt = added["ChangedAt"].ToUniversalTime();
        changedAt.ShouldBeGreaterThanOrEqualTo(Milliseconds(_started));
        changedAt.ShouldBeLessThanOrEqualTo(Milliseconds(DateTime.UtcNow));
        // Every profile field and version must match the complete intended winning document.
        Equal(new BsonDocument(added.Elements.Where(item => item.Name is not "_id" and not "ChangedAt" and not "Action" && item.Name != foreignKey)),
            new BsonDocument(expected.Elements.Where(item => item.Name != "_id")));
        foreach (var name in Collections.Except([collection, historyName]))
            Equal(_committed[name], _before[name]);
        context.RecordEvidence("Atomicity intended aggregate winner", Json(expected));
    }

    internal async Task CommitDeleteAsync(string collection, BsonValue id)
    {
        _before.ShouldNotBeNull();
        _committed = await SnapshotAsync();
        Attach("Atomicity committed winner", _committed);
        _interleavings++;
        _interleavings.ShouldBe(1);
        _before[collection].Count(item => item["_id"] == id).ShouldBe(1);
        Equal(_committed[collection], _before[collection].Where(item => item["_id"] != id));
        foreach (var name in Collections.Except([collection]))
            Equal(_committed[name], _before[name]);
        context.RecordEvidence("Atomicity winning delete verified", new { collection, id = Json(new BsonDocument("_id", id)), historyEffect = "Unchanged; direct competing database delete" });
    }

    internal async Task CommitFavoriteAsync(BsonDocument expected)
    {
        _before.ShouldNotBeNull();
        _committed = await SnapshotAsync();
        Attach("Atomicity committed winner", _committed);
        _interleavings++;
        _interleavings.ShouldBe(1);
        Equal(_committed["favorites"], _before["favorites"].Append(expected));
        foreach (var name in Collections.Except(["favorites"]))
            Equal(_committed[name], _before[name]);
        context.RecordEvidence("Atomicity intended favorite winner", Json(expected));
    }

    internal Task ProblemAsync(int status, string code, string title, string detail)
        => _problem.ExpectProblemAsync(status, code, title, detail, "[]", "[]");

    internal void RecordMongoFailure(MongoWriteException error, string index, string collection)
    {
        error.WriteError.Code.ShouldBe(11000);
        error.WriteError.Message.ShouldContain(index);
        error.WriteError.Message.ShouldContain(collection);
        _mongoFailures++;
        context.RecordEvidence("Atomicity real MongoDB write failure", new { code = error.WriteError.Code, index, collection, manufacturedException = false });
    }

    internal void RecordCreateConflict(ResourceAlreadyExistsException error)
    {
        _createConflict.ShouldBeNull();
        _createConflict = error;
        context.RecordEvidence("Atomicity mapped create conflict", new { error.ActionName, error.ResourceType, error.ResourceId, realRepositoryInvoked = true });
    }

    internal void ExpectCreateConflict(string operation, string entity, string externalId)
    {
        _createConflict.ShouldNotBeNull();
        _createConflict.ActionName.ShouldBe(operation);
        _createConflict.ResourceType.ShouldBe(entity);
        _createConflict.ResourceId.ShouldBe(externalId);
        _mongoFailures.ShouldBe(0);
    }

    internal void ExpectMongoFailure() => _mongoFailures.ShouldBe(1);

    internal async Task ExpectAsync(Table table)
    {
        var expected = table.Rows.ToDictionary(row => row["Field"], row => row["Value"], StringComparer.Ordinal);
        context.RecordEvidence("Atomicity outcome expectations", expected);
        var after = await SnapshotAsync();
        Attach("Atomicity after losing request", after);
        _before.ShouldNotBeNull();
        var rollback = expected["Effect"] == "Rollback";
        expected["Effect"].ShouldBeOneOf("Rollback", "Winner", "Delete");
        _interleavings.ShouldBe(rollback ? 0 : 1);
        var target = rollback ? _before : _committed;
        target.ShouldNotBeNull();
        foreach (var name in Collections)
            Equal(after[name], target[name]);
        var collection = expected["Collection"];
        after[collection].Length.ShouldBe(int.Parse(expected["CurrentCount"], CultureInfo.InvariantCulture));
        if (collection != "favorites")
        {
            after[collection + "-history"].Length.ShouldBe(int.Parse(expected["HistoryCount"], CultureInfo.InvariantCulture));
            if (expected["Version"] != "absent")
                string.Join(",", after[collection].Select(item => item["Version"].ToInt64()).Order()).ShouldBe(expected["Version"]);
            else
                after[collection].ShouldBeEmpty();
        }
        context.RecordEvidence("Atomicity outcome verified", new { effect = expected["Effect"], completeWinnerPreserved = !rollback, allFiveCollectionsChecked = true, interleavings = _interleavings });
    }

    private async Task<Dictionary<string, BsonDocument[]>> SnapshotAsync()
    {
        var database = AcceptanceMongoDb.GetDatabase(context.DatabaseName);
        var snapshot = new Dictionary<string, BsonDocument[]>(StringComparer.Ordinal);
        foreach (var name in Collections)
            snapshot[name] = (await database.GetCollection<BsonDocument>(name).Find(FilterDefinition<BsonDocument>.Empty)
                .Sort(Builders<BsonDocument>.Sort.Ascending("_id")).ToListAsync()).ToArray();
        return snapshot;
    }

    private void Attach(string name, Dictionary<string, BsonDocument[]> snapshot)
        => context.RecordEvidence(name, snapshot.ToDictionary(item => item.Key, item => item.Value.Select(Json).ToArray()));
    private static JsonElement Json(BsonDocument document) => JsonSerializer.Deserialize<JsonElement>(document.ToJson(Canonical));
    private static DateTime Milliseconds(DateTime value) => DateTimeOffset.FromUnixTimeMilliseconds(new DateTimeOffset(value).ToUnixTimeMilliseconds()).UtcDateTime;
    private static void Equal(BsonDocument actual, BsonDocument expected)
        => JsonElement.DeepEquals(Json(actual), Json(expected)).ShouldBeTrue("Every persisted field must match the intended state.");
    private static void Equal(IEnumerable<BsonDocument> actual, IEnumerable<BsonDocument> expected)
        => actual.OrderBy(item => item["_id"].ToString(), StringComparer.Ordinal).Select(item => item.ToJson(Canonical))
            .ShouldBe(expected.OrderBy(item => item["_id"].ToString(), StringComparer.Ordinal).Select(item => item.ToJson(Canonical)));
}

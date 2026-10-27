using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Infrastructure.Context;
using MongoDB.Bson.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using Shouldly;
using System.Text.Json;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Tests;

internal static class MongoStateAssertions
{
    private static readonly string[] Collections = ["customers", "customers-history", "admins", "admins-history", "favorites"];
    private static readonly JsonWriterSettings Canonical = new() { OutputMode = JsonOutputMode.CanonicalExtendedJson };

    internal static async Task<Dictionary<string, BsonDocument[]>> Snapshot(MongoDbContext context)
    {
        var result = new Dictionary<string, BsonDocument[]>();
        foreach (var name in Collections)
            result[name] = (await context.Customers.Database.GetCollection<BsonDocument>(name)
                .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync()).OrderBy(item => item["_id"].ToString(), StringComparer.Ordinal).ToArray();
        return result;
    }

    internal static async Task Unchanged(MongoDbContext context, Dictionary<string, BsonDocument[]> before)
    {
        var after = await Snapshot(context);
        foreach (var name in Collections)
            Equal(after[name], before[name]);
    }

    internal static async Task Winner(MongoDbContext context, Dictionary<string, BsonDocument[]> before, string collection, BsonDocument expected)
    {
        var after = await Snapshot(context);
        var id = expected["_id"];
        var previous = before[collection].SingleOrDefault(item => item["_id"] == id);
        expected["Version"].ToInt64().ShouldBe(previous is null ? 0 : previous["Version"].ToInt64() + 1);
        Equal(after[collection], before[collection].Where(item => item["_id"] != id).Append(expected));
        var historyName = collection + "-history";
        after[historyName].Length.ShouldBe(before[historyName].Length + 1);
        foreach (var old in before[historyName])
            Equal(after[historyName].Single(item => item["_id"] == old["_id"]), old);
        var history = after[historyName].Single(item => !before[historyName].Any(old => old["_id"] == item["_id"]));
        var foreignKey = collection == "customers" ? "CustomerId" : "AdminId";
        history[foreignKey].ShouldBe(id);
        history["_id"].ShouldNotBe(new BsonBinaryData(Guid.Empty, GuidRepresentation.Standard));
        history["ChangedAt"].ToUniversalTime().ShouldBeGreaterThan(DateTime.UnixEpoch);
        history["Action"].ToInt32().ShouldBe((int)(previous is null ? ActionType.Insert : ActionType.Update));
        Equal(new BsonDocument(history.Elements.Where(item => item.Name is not "_id" and not "ChangedAt" and not "Action" && item.Name != foreignKey)),
            new BsonDocument(expected.Elements.Where(item => item.Name != "_id")));
        foreach (var name in Collections.Except([collection, historyName]))
            Equal(after[name], before[name]);
    }

    internal static void Equal(BsonDocument actual, BsonDocument expected)
        => JsonElement.DeepEquals(JsonSerializer.Deserialize<JsonElement>(actual.ToJson(Canonical)),
            JsonSerializer.Deserialize<JsonElement>(expected.ToJson(Canonical))).ShouldBeTrue("Every document field must match.");

    private static void Equal(IEnumerable<BsonDocument> actual, IEnumerable<BsonDocument> expected)
        => actual.OrderBy(item => item["_id"].ToString(), StringComparer.Ordinal).Select(item => item.ToJson(Canonical))
            .ShouldBe(expected.OrderBy(item => item["_id"].ToString(), StringComparer.Ordinal).Select(item => item.ToJson(Canonical)));
}

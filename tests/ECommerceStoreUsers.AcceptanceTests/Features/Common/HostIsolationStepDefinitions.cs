using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class HostIsolationStepDefinitions(ScenarioApiContext context, FrameworkFailureState state)
{
    private string? _peerName;
    private ApplicationFactory? _peer;
    private HttpClient? _peerClient;
    private CreateAdminRequestDto[] _requests = [];
    private AdminResponseDto[] _responses = [];
    private Dictionary<string, JsonElement[]>? _mainAfterCreate;
    private bool _removed;

    [Given("a peer Users host uses another scenario database")]
    public async Task GivenPeer()
    {
        _peerName = $"acceptance-{Guid.NewGuid():N}";
        _peerName.ShouldNotBe(context.DatabaseName);
        _peer = new ApplicationFactory(AcceptanceMongoDb.ConnectionString, _peerName);
        _peerClient = context.CreateClient(_peer);
        var main = await state.SnapshotAsync();
        var peer = await state.SnapshotAsync(_peerName);
        main.Values.ShouldAllBe(documents => documents.Length == 0);
        peer.Values.ShouldAllBe(documents => documents.Length == 0);
        context.RecordEvidence("Technical two-host setup", new { mainDatabase = context.DatabaseName, peerDatabase = _peerName, main, peer });
    }

    [When("both hosts create the same external identifier with these profiles")]
    public async Task WhenCreates(Table table)
    {
        table.Rows.Select(row => row["Host"]).ShouldBe(new[] { "Main", "Peer" });
        _requests = table.Rows.Select(row => new CreateAdminRequestDto
        {
            ExternalId = row["ExternalId"],
            FullName = row["FullName"],
            Email = row["Email"]
        }).ToArray();
        _requests.Select(request => request.ExternalId).Distinct().ShouldHaveSingleItem();
        _peerClient.ShouldNotBeNull();
        var responses = new List<AdminResponseDto>();
        foreach (var (client, request) in new[] { context.HttpClient, _peerClient }.Zip(_requests))
        {
            using var response = await client.PostAsJsonAsync("/admins", request, context.JsonOptions);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
            responses.Add(JsonSerializer.Deserialize<AdminResponseDto>(await response.Content.ReadAsStringAsync(), context.JsonOptions)!);
        }
        _responses = responses.ToArray();
        _responses[0].Id.ShouldNotBe(_responses[1].Id);
        context.RecordEvidence("Technical two-host HTTP results", new { requests = _requests, responses = _responses });
    }

    [Then("each host has exactly its own complete Admin and Insert history")]
    public async Task ThenIsolated()
    {
        _peerName.ShouldNotBeNull();
        _mainAfterCreate = await state.SnapshotAsync();
        var peer = await state.SnapshotAsync(_peerName);
        context.RecordEvidence("Technical two-host after creates", new { main = _mainAfterCreate, peer });
        VerifyProfile(_mainAfterCreate, _requests[0], _responses[0]);
        VerifyProfile(peer, _requests[1], _responses[1]);
        context.RecordEvidence("Technical two-host isolation verified", new { bothStatuses = new[] { 200, 200 }, distinctDatabases = true, distinctAdminIds = true, exactCurrentAndHistory = true });
    }

    private void VerifyProfile(Dictionary<string, JsonElement[]> snapshot, CreateAdminRequestDto request, AdminResponseDto response)
    {
        response.Id.ShouldNotBe(Guid.Empty);
        response.LastLoginAt.ShouldNotBe(default);
        response.LastLoginAt.Kind.ShouldBe(DateTimeKind.Utc);
        var expectedResponse = new { response.Id, request.ExternalId, request.FullName, request.Email, IsActive = true, response.LastLoginAt };
        JsonElement.DeepEquals(JsonSerializer.SerializeToElement(response, context.JsonOptions), JsonSerializer.SerializeToElement(expectedResponse, context.JsonOptions)).ShouldBeTrue();
        foreach (var name in snapshot.Keys.Except(["admins", "admins-history"]))
            snapshot[name].ShouldBeEmpty();
        var expectedCurrent = new BsonDocument
        {
            ["_id"] = new BsonBinaryData(response.Id, GuidRepresentation.Standard),
            ["ExternalId"] = request.ExternalId,
            ["FullName"] = request.FullName,
            ["Email"] = request.Email,
            ["IsActive"] = true,
            ["LastLoginAt"] = new BsonDateTime(response.LastLoginAt),
            ["Version"] = new BsonInt64(0)
        };
        Equal(BsonDocument.Parse(snapshot["admins"].ShouldHaveSingleItem().GetRawText()), expectedCurrent);
        var history = BsonDocument.Parse(snapshot["admins-history"].ShouldHaveSingleItem().GetRawText());
        history["AdminId"].ShouldBe(expectedCurrent["_id"]);
        history["Action"].ToInt32().ShouldBe(0);
        history["_id"].AsBsonBinaryData.ToGuid(GuidRepresentation.Standard).ShouldNotBe(Guid.Empty);
        history["ChangedAt"].ToUniversalTime().ShouldBeGreaterThan(DateTime.UnixEpoch);
        foreach (var field in new[] { "_id", "AdminId", "ChangedAt", "Action" })
            history.Remove(field);
        expectedCurrent.Remove("_id");
        Equal(history, expectedCurrent);
    }

    [When("the peer host is stopped and its database is removed")]
    public async Task WhenDropPeer()
    {
        _peerName.ShouldNotBeNull();
        var mongo = context.Factory.Services.GetRequiredService<MongoDbContext>();
        (await (await mongo.Client.ListDatabaseNamesAsync()).ToListAsync()).ShouldContain(_peerName);
        await CleanupPeer();
        (await (await mongo.Client.ListDatabaseNamesAsync()).ToListAsync()).ShouldNotContain(_peerName);
        context.RecordEvidence("Technical peer cleanup verified", new { peerDatabase = _peerName, hostDisposed = true, databaseAbsent = true });
    }

    [Then("the main host still returns its full profile with unchanged stored data")]
    public async Task ThenMainRemains()
    {
        _removed.ShouldBeTrue();
        context.Response = await context.HttpClient.GetAsync("/admins/external/shared-isolated");
        context.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        context.Response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        var response = JsonSerializer.Deserialize<AdminResponseDto>(await context.Response.Content.ReadAsStringAsync(), context.JsonOptions)!;
        JsonElement.DeepEquals(JsonSerializer.SerializeToElement(response), JsonSerializer.SerializeToElement(_responses[0])).ShouldBeTrue();
        var after = await state.SnapshotAsync();
        JsonElement.DeepEquals(JsonSerializer.SerializeToElement(after), JsonSerializer.SerializeToElement(_mainAfterCreate)).ShouldBeTrue();
        context.RecordEvidence("Technical main after peer cleanup", new { response, collections = after, unchanged = true });
    }

    [AfterScenario]
    public async Task CleanupPeer()
    {
        if (_peerName is null || _removed)
            return;
        _peerClient?.Dispose();
        if (_peer is not null)
            await _peer.DisposeAsync();
        await AcceptanceMongoDb.DropDatabaseAsync(_peerName);
        _removed = true;
    }

    private static void Equal(BsonDocument actual, BsonDocument expected)
    {
        var json = new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson };
        JsonElement.DeepEquals(JsonSerializer.Deserialize<JsonElement>(actual.ToJson(json)), JsonSerializer.Deserialize<JsonElement>(expected.ToJson(json))).ShouldBeTrue();
    }
}

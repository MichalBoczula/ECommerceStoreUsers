using ECommerceStoreUsers.Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class TechnicalStepDefinitions(ScenarioApiContext context, FrameworkFailureState state)
{
    [Given("versioned Users data exists before a technical operation")]
    public async Task GivenVersionedData()
    {
        state.UseEvidencePrefix("Technical");
        await state.SeedAsync();
        await state.BeginAsync();
    }

    [Then("the technical operation leaves all Users collections unchanged")]
    public Task ThenUnchanged() => state.ExpectUnchangedAsync();

    [Then("the complete technical server problem is safe")]
    public Task ThenProblem() => state.ExpectProblemAsync(500, "internal_error", "Server error.",
        "An unexpected error occurred.", "[]", "[]");

    [Then("the real MongoDB topology supports transactional writes")]
    public async Task ThenRealTopology()
    {
        var mongo = context.Factory.Services.GetRequiredService<MongoDbContext>();
        var hello = await mongo.Client.GetDatabase("admin").RunCommandAsync<BsonDocument>(new BsonDocument("hello", 1));
        var ping = await mongo.Customers.Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
        hello["setName"].AsString.ShouldNotBeNullOrWhiteSpace();
        hello["isWritablePrimary"].AsBoolean.ShouldBeTrue();
        hello["logicalSessionTimeoutMinutes"].ToDouble().ShouldBeGreaterThan(0);
        ping["ok"].ToDouble().ShouldBe(1);
        var json = new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson };
        context.RecordEvidence("Technical real MongoDB topology", new
        {
            database = context.DatabaseName,
            hello = JsonSerializer.Deserialize<JsonElement>(hello.ToJson(json)),
            ping = JsonSerializer.Deserialize<JsonElement>(ping.ToJson(json)),
            scope = "Independent topology check alongside the actual readiness HTTP response"
        });
    }
}

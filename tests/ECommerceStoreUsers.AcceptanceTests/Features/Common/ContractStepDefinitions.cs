using ECommerceStoreUsers.AcceptanceTests.Features.Customers;
using Reqnroll;
using Shouldly;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class ContractStepDefinitions(ScenarioApiContext context, CustomerSuccessState customers, FrameworkFailureState database)
{
    private Table? _fields;
    private string? _request;
    private JsonDocument? _published;

    [Given("the published contract includes these fields")]
    public void GivenFields(Table table)
    {
        _fields = table;
        context.RecordEvidence("Published contract field expectations", table.Rows.Select(row => row.ToDictionary(pair => pair.Key, pair => pair.Value)).ToArray());
    }

    [When("I read the generated Users OpenAPI document")]
    public async Task WhenReading()
    {
        context.Response = await context.HttpClient.GetAsync("/swagger/v1/swagger.json");
        context.Response.EnsureSuccessStatusCode();
        _published = JsonDocument.Parse(await context.Response.Content.ReadAsByteArrayAsync());
    }

    [Then("every generated DTO field matches serializer metadata and its typed example")]
    public void ThenMetadata()
    {
        _published.ShouldNotBeNull();
        _fields.ShouldNotBeNull();
        var schemas = _published.RootElement.GetProperty("components").GetProperty("schemas");
        foreach (var row in _fields.Rows)
        {
            var owner = schemas.GetProperty(row["Schema"]);
            var field = owner.GetProperty("properties").GetProperty(row["Field"]);
            var shape = field;
            if (shape.TryGetProperty("allOf", out var all)) shape = all[0];
            if (shape.TryGetProperty("$ref", out var reference)) shape = schemas.GetProperty(reference.GetString()!.Split('/').Last());
            shape.GetProperty("type").GetString().ShouldBe(row["Type"]);
            (owner.TryGetProperty("required", out var required) && required.EnumerateArray().Any(item => item.GetString() == row["Field"]))
                .ShouldBe(bool.Parse(row["Required"]));
            (field.TryGetProperty("nullable", out var nullable) && nullable.GetBoolean()).ShouldBe(bool.Parse(row["Nullable"]));
            (shape.TryGetProperty("format", out var format) ? format.GetString() : "<none>").ShouldBe(row["Format"]);
        }
        context.RecordEvidence("Published contract metadata verified", OpenApiContractAssertions.Check(_published));
    }

    [Given("the serializer Customer JSON is")]
    public void GivenJson(string json) => _request = json;

    [Given("the companies representation is {string}")]
    public void GivenCompanies(string representation)
    {
        var body = JsonNode.Parse(_request!)!.AsObject();
        switch (representation)
        {
            case "omitted": body.Remove("companies"); break;
            case "null": body["companies"] = null; break;
            case "empty": break;
            case "case-insensitive-with-unknown":
                body["unknownRoot"] = new JsonObject { ["ignored"] = true };
                body["individual"]!["billingAddress"]!["unknownNested"] = 123;
                Uppercase(body);
                break;
            default: throw new InvalidOperationException($"Unknown serializer representation: {representation}");
        }
        _request = body.ToJsonString();
        context.RecordEvidence("Serializer request expectations", new { representation, method = "POST", path = "/customers", body = body.DeepClone() });
    }

    [When("I submit the exact serializer Customer request")]
    public async Task WhenCreating()
    {
        _request.ShouldNotBeNull();
        var before = await database.SnapshotAsync();
        before.Values.ShouldAllBe(items => items.Length == 0);
        context.RecordEvidence("Serializer all collections before", before);
        await customers.BeginAsync("Create");
        context.Response = await context.HttpClient.PostAsync("/customers", new StringContent(_request, Encoding.UTF8, "application/json"));
        customers.RequestCompleted();
        var after = await database.SnapshotAsync();
        foreach (var collection in new[] { "admins", "admins-history", "favorites" })
            after[collection].ShouldBeEmpty();
        context.RecordEvidence("Serializer all collections after", after);
    }

    [AfterScenario]
    public void DisposeDocument() => _published?.Dispose();

    private static void Uppercase(JsonNode node)
    {
        if (node is JsonArray array)
            foreach (var child in array.OfType<JsonNode>()) Uppercase(child);
        if (node is not JsonObject obj)
            return;
        foreach (var pair in obj.ToArray())
        {
            obj.Remove(pair.Key);
            if (pair.Value is not null) Uppercase(pair.Value);
            obj[pair.Key.ToUpperInvariant()] = pair.Value;
        }
    }
}

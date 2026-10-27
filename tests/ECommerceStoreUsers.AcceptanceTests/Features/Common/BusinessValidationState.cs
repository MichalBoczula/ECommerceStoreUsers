using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

/// <summary>Explicit valid Gherkin payloads with isolated JSON-value changes.</summary>
public sealed class BusinessValidationState
{
    private readonly ScenarioApiContext _context;
    private readonly FrameworkFailureState _rejection;
    private readonly Dictionary<string, JsonNode> _templates = new(StringComparer.Ordinal);
    private JsonNode? _body;
    private string? _method;
    private string? _path;

    public BusinessValidationState(ScenarioApiContext context)
    {
        _context = context;
        _rejection = new FrameworkFailureState(context);
        _rejection.UseEvidencePrefix("Business validation");
    }

    internal Task SeedAsync() => _rejection.SeedAsync();
    internal Task BeginAsync() => _rejection.BeginAsync();
    internal Task UnchangedAsync() => _rejection.ExpectUnchangedAsync();

    internal void Template(string family, string json)
    {
        var value = JsonNode.Parse(json);
        value.ShouldBeOfType<JsonObject>();
        _templates.Add(family, value!);
        _context.RecordEvidence("Business validation valid " + family + " template", value!);
    }

    internal void Target(string operationId, string method, string path)
    {
        _method = method;
        _path = path;
        _body = operationId switch
        {
            "CreateCustomer" => _templates["Customer"].DeepClone(),
            "UpdateIndividualData" => new JsonObject { ["individual"] = _templates["Customer"]["individual"]!.DeepClone() },
            "AddCompany" or "UpdateCompany" => _templates["Customer"]["companies"]![0]!.DeepClone(),
            "CreateAdmin" or "UpdateAdminProfile" => _templates["Admin"].DeepClone(),
            "AddProductToFavorites" => _templates["Favorite"].DeepClone(),
            "GetCustomerByExternalId" or "GetAdminByExternalId" or "GetFavoritesByClientId" or
                "RemoveProductFromFavorites" or "ClearClientFavorites" => null,
            _ => throw new ArgumentOutOfRangeException(nameof(operationId))
        };
        if (operationId == "UpdateAdminProfile")
            _body!.AsObject().Remove("externalId").ShouldBeTrue();
        _context.RecordEvidence("Business validation operation", new { operationId, method, path });
    }

    internal void Change(Table table)
    {
        _body.ShouldNotBeNull();
        var changes = table.Rows.Select(row => new { pointer = row["Pointer"], jsonValue = row["JsonValue"] }).ToArray();
        _context.RecordEvidence("Business validation input changes", changes);
        foreach (var change in changes)
        {
            var segments = change.pointer.Split('/');
            segments[0].ShouldBeEmpty();
            segments.Length.ShouldBeGreaterThan(1);
            var parent = _body;
            foreach (var segment in segments.Skip(1).SkipLast(1))
            {
                parent = Child(parent!, segment);
                parent.ShouldNotBeNull();
            }
            var key = segments[^1];
            var value = JsonNode.Parse(change.jsonValue);
            if (parent is JsonObject obj)
            {
                obj.ContainsKey(key).ShouldBeTrue("Override an existing member; do not invent request fields.");
                obj[key] = value;
            }
            else
                parent!.AsArray()[int.Parse(key, CultureInfo.InvariantCulture)] = value;
        }
    }

    internal async Task SendAsync()
    {
        _method.ShouldNotBeNull();
        _path.ShouldNotBeNull();
        var body = _body?.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) ?? "<no-content>";
        _rejection.Prepare(_method, _path, body, _body is null ? "<none>" : "application/json; charset=utf-8");
        await _rejection.SendAsync();
    }

    internal async Task ValidationAsync(Table table)
    {
        var errors = table.Rows.Select(row => new
        {
            name = row["Name"],
            message = row["Message"],
            entity = row["Entity"]
        }).ToArray();
        errors.ShouldNotBeEmpty();
        await _rejection.ExpectProblemAsync(400, "validation_failed", "Validation failed.",
            "One or more validation errors occurred.", "[]", "[]", JsonSerializer.Serialize(errors));
    }

    internal Task ConflictAsync(string detail)
        => _rejection.ExpectProblemAsync(409, "resource_conflict", "Conflict.", detail, "[]", "[]");

    internal Task MappingFailureAsync()
        => _rejection.ExpectProblemAsync(500, "internal_error", "Server error.", "An unexpected error occurred.", "[]", "[]");

    internal async Task AddSecondCompanyAsync()
    {
        var request = _templates["Customer"]["companies"]![1]!.DeepClone();
        using var response = await _context.HttpClient.PostAsJsonAsync($"/customers/{_rejection.CustomerId}/companies", request, _context.JsonOptions);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _context.RecordEvidence("Business validation second company setup", new
        {
            request,
            response = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync())
        });
    }

    private static JsonNode? Child(JsonNode parent, string key)
        => parent is JsonObject obj ? obj[key] : parent.AsArray()[int.Parse(key, CultureInfo.InvariantCulture)];
}

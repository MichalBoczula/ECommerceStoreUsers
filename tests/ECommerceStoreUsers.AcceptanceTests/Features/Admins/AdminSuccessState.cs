using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Persistence.Admins;
using ECommerceStoreUsers.Infrastructure.Persistence.Admins.History;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins;

/// <summary>Scenario-local complete Admin profiles and actual MongoDB current/history evidence.</summary>
public sealed class AdminSuccessState(ScenarioApiContext context)
{
    private static readonly JsonSerializerOptions EvidenceJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private Snapshot? _before;
    private Snapshot? _after;
    private AdminResponseDto? _response;
    private string? _seedExternalId;
    private string _operation = string.Empty;
    private string _prefix = "Admin";
    private DateTime _started;
    private DateTime _finished;

    internal static Dictionary<string, string> Values(Table table)
        => table.Rows.ToDictionary(row => row["Field"], row => row["Value"], StringComparer.Ordinal);

    internal static string Required(IReadOnlyDictionary<string, string> values, string field)
        => values.TryGetValue(field, out var value) ? value
            : throw new InvalidOperationException($"Missing explicit Admin fixture/expectation: {field}");

    internal static CreateAdminRequestDto CreateRequest(Table table)
    {
        var values = Values(table);
        return new CreateAdminRequestDto
        {
            ExternalId = Required(values, "ExternalId"),
            FullName = Required(values, "FullName"),
            Email = Required(values, "Email")
        };
    }

    internal static UpdateAdminProfileRequestDto UpdateRequest(Table table)
    {
        var values = Values(table);
        return new UpdateAdminProfileRequestDto
        {
            FullName = Required(values, "FullName"),
            Email = Required(values, "Email")
        };
    }

    internal void Attach(string name, object value) => context.RecordEvidence(name, value);

    internal Task<AdminResponseDto> SeedAsync(Table table) => SeedAsync(CreateRequest(table));

    internal async Task<AdminResponseDto> SeedAsync(CreateAdminRequestDto request)
    {
        _seedExternalId = request.ExternalId;
        Attach("Admin setup request", request);
        var started = DateTime.UtcNow;
        using var response = await context.HttpClient.PostAsJsonAsync("/admins", request, context.JsonOptions);
        var finished = DateTime.UtcNow;
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        var admin = await response.Content.ReadFromJsonAsync<AdminResponseDto>(context.JsonOptions);
        admin.ShouldNotBeNull();
        AssertProfile(admin, request.ExternalId, request.FullName, request.Email, true);
        InWindow(admin.LastLoginAt, started, finished);
        var snapshot = await LoadAsync();
        Attach("Admin setup database", JsonSerializer.SerializeToElement(snapshot, EvidenceJson));
        var current = snapshot.Current.ShouldHaveSingleItem();
        var history = snapshot.History.ShouldHaveSingleItem();
        current.Id.ShouldBe(admin.Id);
        current.Version.ShouldBe(0);
        AssertEqual(Profile(current), Profile(admin));
        history.Id.ShouldNotBe(Guid.Empty);
        history.AdminId.ShouldBe(current.Id);
        history.Version.ShouldBe(0);
        history.Action.ShouldBe(ActionType.Insert);
        InWindow(history.ChangedAt, started, finished);
        AssertEqual(Profile(history), Profile(current));
        return admin;
    }

    internal async Task BeginAsync(string operation, string prefix = "Admin")
    {
        _operation = operation;
        _prefix = prefix;
        _before = await LoadAsync();
        _after = null;
        _response = null;
        Attach(_prefix + " database before", JsonSerializer.SerializeToElement(_before, EvidenceJson));
        _started = DateTime.UtcNow;
    }

    internal async Task CompleteAsync()
    {
        _finished = DateTime.UtcNow;
        _after = await LoadAsync();
        // Capture the resulting documents before response assertions can fail.
        Attach(_prefix + " database after", JsonSerializer.SerializeToElement(_after, EvidenceJson));
    }

    internal Task ExpectResponseAsync(Table table) => ExpectResponseAsync(Values(table));

    private async Task ExpectResponseAsync(IReadOnlyDictionary<string, string> expected)
    {
        Attach(_prefix + " response expectations", expected);
        Required(expected, "HasId").ShouldBe("true");
        Required(expected, "HasLastLoginAt").ShouldBe("true");
        context.Response.ShouldNotBeNull();
        context.Response.StatusCode.ShouldBe((HttpStatusCode)Number(expected, "StatusCode"));
        context.Response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        _response = await context.Response.Content.ReadFromJsonAsync<AdminResponseDto>(context.JsonOptions);
        _response.ShouldNotBeNull();
        var externalId = Required(expected, "ExternalId");
        if (externalId == "<setup-external-id>")
            externalId = _seedExternalId ?? throw new InvalidOperationException("No explicit setup request identity was recorded.");
        AssertProfile(_response, externalId, Required(expected, "FullName"), Required(expected, "Email"),
            bool.Parse(Required(expected, "IsActive")));
        _before.ShouldNotBeNull();
        if (_before.Current.Length != 0)
            _response.Id.ShouldBe(_before.Current.ShouldHaveSingleItem().Id);
    }

    internal Task ExpectDatabaseAsync(Table table) => ExpectDatabaseAsync(Values(table));

    private Task ExpectDatabaseAsync(IReadOnlyDictionary<string, string> expected)
    {
        Attach(_prefix + " persistence expectations", expected);
        _before.ShouldNotBeNull();
        _after.ShouldNotBeNull();
        _response.ShouldNotBeNull();
        _before.Current.Length.ShouldBe(Number(expected, "CurrentBefore"));
        _before.History.Length.ShouldBe(Number(expected, "HistoryBefore"));
        _after.Current.Length.ShouldBe(Number(expected, "CurrentAfter"));
        _after.History.Length.ShouldBe(Number(expected, "HistoryAfter"));
        var current = _after.Current.ShouldHaveSingleItem();
        current.Id.ShouldBe(_response.Id);
        current.Version.ShouldBe(Number(expected, "VersionAfter"));
        AssertEqual(Profile(current), Profile(_response));
        if (Required(expected, "VersionBefore") == "none")
            _before.Current.ShouldBeEmpty();
        else
            _before.Current.ShouldHaveSingleItem().Version.ShouldBe(Number(expected, "VersionBefore"));
        foreach (var previous in _before.History)
            AssertEqual(_after.History.Single(item => item.Id == previous.Id), previous);
        var actions = Required(expected, "HistoryActions").Split(',').Select(Enum.Parse<ActionType>).ToArray();
        _after.History.Select(item => item.Action).ShouldBe(actions);
        _after.History.Select(item => item.Version).ShouldBe(Enumerable.Range(0, actions.Length).Select(value => (long)value));
        _after.History.Select(item => item.Id).Distinct().Count().ShouldBe(_after.History.Length);
        foreach (var history in _after.History)
        {
            history.Id.ShouldNotBe(Guid.Empty);
            history.AdminId.ShouldBe(current.Id);
            history.ChangedAt.ShouldNotBe(default);
            history.ChangedAt.Kind.ShouldBe(DateTimeKind.Utc);
        }
        var latest = _after.History.Last();
        latest.Version.ShouldBe(current.Version);
        AssertEqual(Profile(latest), Profile(current));
        var effect = Required(expected, "WriteEffect");
        if (effect == "None")
            AssertEqual(_after, _before);
        else
        {
            (effect is "Insert" or "Update").ShouldBeTrue();
            latest.Action.ShouldBe(Enum.Parse<ActionType>(effect));
            (_after.History.Length - _before.History.Length).ShouldBe(1);
            InWindow(latest.ChangedAt, _started, _finished);
            if (_before.Current.Length != 0)
            {
                current.Version.ShouldBe(_before.Current[0].Version + 1);
                AssertEqual(Profile(_before.History.Last()), Profile(_before.Current[0]));
            }
        }
        var login = Required(expected, "LastLoginAt");
        if (login == "generated")
            InWindow(current.LastLoginAt, _started, _finished);
        else
        {
            login.ShouldBe("retained");
            current.LastLoginAt.ShouldBe(_before.Current.ShouldHaveSingleItem().LastLoginAt);
        }
        if (_before.Current.Length != 0)
        {
            current.ExternalId.ShouldBe(_before.Current[0].ExternalId);
            current.IsActive.ShouldBe(_before.Current[0].IsActive);
        }
        Attach(_prefix + " persistence verified", new
        {
            operation = _operation,
            adminId = current.Id,
            current.Version,
            currentCount = _after.Current.Length,
            historyCount = _after.History.Length,
            writeEffect = effect,
            lastLoginAt = login,
            previousHistoryPreserved = true,
            requestStartedAt = _started,
            requestCompletedAt = _finished
        });
        return Task.CompletedTask;
    }

    internal async Task ApplyPriorUpdateAsync(Guid adminId, UpdateAdminProfileRequestDto request)
    {
        await BeginAsync("Update", "Admin prior update");
        context.Response = await context.HttpClient.PutAsJsonAsync($"/admins/{adminId}", request, context.JsonOptions);
        await CompleteAsync();
        await ExpectResponseAsync(new Dictionary<string, string>
        {
            ["StatusCode"] = "200",
            ["HasId"] = "true",
            ["ExternalId"] = "<setup-external-id>",
            ["FullName"] = request.FullName,
            ["Email"] = request.Email,
            ["IsActive"] = "true",
            ["HasLastLoginAt"] = "true"
        });
        await ExpectDatabaseAsync(new Dictionary<string, string>
        {
            ["CurrentBefore"] = "1",
            ["HistoryBefore"] = "1",
            ["VersionBefore"] = "0",
            ["CurrentAfter"] = "1",
            ["HistoryAfter"] = "2",
            ["VersionAfter"] = "1",
            ["HistoryActions"] = "Insert,Update",
            ["WriteEffect"] = "Update",
            ["LastLoginAt"] = "retained"
        });
        context.Response.Dispose();
        context.Response = null;
    }

    private async Task<Snapshot> LoadAsync()
    {
        var mongo = context.Factory.Services.GetRequiredService<MongoDbContext>();
        var current = await mongo.Admins.Find(FilterDefinition<AdminDocument>.Empty).ToListAsync();
        var history = await mongo.AdminsHistory.Find(FilterDefinition<AdminHistoryDocument>.Empty).ToListAsync();
        return new Snapshot(current.OrderBy(item => item.Id).ToArray(), history.OrderBy(item => item.Version).ThenBy(item => item.Id).ToArray());
    }

    private static int Number(IReadOnlyDictionary<string, string> values, string field)
        => int.Parse(Required(values, field), CultureInfo.InvariantCulture);

    private static void AssertProfile(AdminResponseDto actual, string externalId, string name, string email, bool active)
    {
        actual.Id.ShouldNotBe(Guid.Empty);
        actual.ExternalId.ShouldBe(externalId);
        actual.FullName.ShouldBe(name);
        actual.Email.ShouldBe(email);
        actual.IsActive.ShouldBe(active);
        actual.LastLoginAt.ShouldNotBe(default);
        actual.LastLoginAt.Kind.ShouldBe(DateTimeKind.Utc);
    }

    private static void InWindow(DateTime value, DateTime started, DateTime finished)
    {
        value.Kind.ShouldBe(DateTimeKind.Utc);
        var actual = new DateTimeOffset(value).ToUnixTimeMilliseconds();
        actual.ShouldBeGreaterThanOrEqualTo(new DateTimeOffset(started).ToUnixTimeMilliseconds());
        actual.ShouldBeLessThanOrEqualTo(new DateTimeOffset(finished).ToUnixTimeMilliseconds());
    }

    private static object Profile(AdminDocument admin)
        => new { admin.ExternalId, admin.FullName, admin.Email, admin.IsActive, admin.LastLoginAt };
    private static object Profile(AdminHistoryDocument admin)
        => new { admin.ExternalId, admin.FullName, admin.Email, admin.IsActive, admin.LastLoginAt };
    private static object Profile(AdminResponseDto admin)
        => new
        {
            admin.ExternalId,
            admin.FullName,
            admin.Email,
            admin.IsActive,
            LastLoginAt = DateTimeOffset.FromUnixTimeMilliseconds(new DateTimeOffset(admin.LastLoginAt).ToUnixTimeMilliseconds()).UtcDateTime
        };

    private static void AssertEqual(object actual, object expected)
        => JsonElement.DeepEquals(JsonSerializer.SerializeToElement(actual, EvidenceJson),
            JsonSerializer.SerializeToElement(expected, EvidenceJson)).ShouldBeTrue("Complete Admin data and metadata must match the explicit expectation.");

    private sealed record Snapshot(AdminDocument[] Current, AdminHistoryDocument[] History);
}

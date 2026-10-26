using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Persistence.Customers;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers;

/// <summary>Shared fixture parsing and real current/history evidence for customer success scenarios.</summary>
public sealed class CustomerSuccessState(ScenarioApiContext context)
{
    private static readonly JsonSerializerOptions EvidenceJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private Snapshot? _before;
    private CustomerResponseDto? _response;
    private string _operation = string.Empty;
    private DateTime _started;
    private DateTime _finished;
    internal Guid TargetCompanyId { get; set; }

    internal static Dictionary<string, string> Values(Table table)
        => table.Rows.ToDictionary(row => row["Field"], row => row["Value"], StringComparer.Ordinal);

    internal static string Required(IReadOnlyDictionary<string, string> values, string field)
        => values.TryGetValue(field, out var value) ? value
            : throw new InvalidOperationException($"Missing explicit customer expectation/fixture field: {field}");

    internal static CreateCustomerRequestDto Customer(IReadOnlyDictionary<string, string> values) => new()
    {
        ExternalId = Required(values, "ExternalId"),
        Individual = Individual(values),
        Companies = CompanyPrefixes(values).Select(prefix => Company(values, prefix)).ToArray()
    };

    internal static IndividualDataRequestDto Individual(IReadOnlyDictionary<string, string> values) => new()
    {
        FirstName = Required(values, "Individual.FirstName"),
        LastName = Required(values, "Individual.LastName"),
        Email = Required(values, "Individual.Email"),
        Phone = Required(values, "Individual.Phone"),
        BillingAddress = Address(values, "Individual.BillingAddress"),
        ShippingAddress = Address(values, "Individual.ShippingAddress")
    };

    internal static AddCompanyRequestDto Company(IReadOnlyDictionary<string, string> values, string prefix = "Company") => new()
    {
        TaxId = Required(values, prefix + ".TaxId"),
        CompanyName = Required(values, prefix + ".CompanyName"),
        BillingAddress = Address(values, prefix + ".BillingAddress"),
        ShippingAddress = Address(values, prefix + ".ShippingAddress")
    };

    private static AddressRequestDto Address(IReadOnlyDictionary<string, string> values, string prefix)
    {
        var apartment = Required(values, prefix + ".ApartmentNumber");
        return new AddressRequestDto
        {
            PostalCode = Required(values, prefix + ".PostalCode"),
            City = Required(values, prefix + ".City"),
            Street = Required(values, prefix + ".Street"),
            BuildingNumber = Required(values, prefix + ".BuildingNumber"),
            ApartmentNumber = apartment == "<null>" ? null : apartment
        };
    }

    private static IEnumerable<string> CompanyPrefixes(IReadOnlyDictionary<string, string> values)
        => values.Keys.Where(key => Regex.IsMatch(key, @"^Company(?:\d+)?\.TaxId$"))
            .Select(key => key[..key.IndexOf('.')]).Order(StringComparer.Ordinal);

    internal void Attach(string name, object value) => context.RecordEvidence(name, value);

    internal async Task<CustomerResponseDto> SeedAsync(Table table)
    {
        var request = Customer(Values(table));
        Attach("Customer setup request", request);
        using var response = await context.HttpClient.PostAsJsonAsync("/customers", request, context.JsonOptions);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponseDto>(context.JsonOptions);
        customer.ShouldNotBeNull();
        AssertProfile(customer, request);
        var snapshot = await LoadAsync();
        Attach("Customer setup database", JsonSerializer.SerializeToElement(snapshot, EvidenceJson));
        var current = snapshot.Current.ShouldHaveSingleItem();
        var history = snapshot.History.ShouldHaveSingleItem();
        current.Id.ShouldBe(customer.Id);
        current.Version.ShouldBe(0);
        AssertEqual(StoredProfile(current), StoredProfile(customer));
        history.Id.ShouldNotBe(Guid.Empty);
        history.CustomerId.ShouldBe(current.Id);
        history.Version.ShouldBe(0);
        history.Action.ShouldBe(ActionType.Insert);
        history.ChangedAt.ShouldNotBe(default);
        history.ChangedAt.Kind.ShouldBe(DateTimeKind.Utc);
        AssertEqual(StoredProfile(history), StoredProfile(current));
        return customer;
    }

    internal async Task BeginAsync(string operation)
    {
        _operation = operation;
        _before = await LoadAsync();
        Attach("Customer database before", JsonSerializer.SerializeToElement(_before, EvidenceJson));
        _started = DateTime.UtcNow;
    }

    internal void RequestCompleted() => _finished = DateTime.UtcNow;

    internal async Task ExpectResponseAsync(Table table)
    {
        var expected = Values(table);
        Attach("Customer response expectations", expected);
        Required(expected, "HasId").ShouldBe("true");
        Required(expected, "HasUpdatedAt").ShouldBe("true");
        context.Response.ShouldNotBeNull();
        context.Response.StatusCode.ShouldBe((HttpStatusCode)int.Parse(Required(expected, "StatusCode"), CultureInfo.InvariantCulture));
        context.Response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        _response = await context.Response.Content.ReadFromJsonAsync<CustomerResponseDto>(context.JsonOptions);
        _response.ShouldNotBeNull();
        var profile = Customer(expected);
        profile.Companies.Count.ShouldBe(int.Parse(Required(expected, "CompaniesCount"), CultureInfo.InvariantCulture));
        AssertProfile(_response, profile);
        _before.ShouldNotBeNull();
        if (_before.Current.Length != 0)
            _response.Id.ShouldBe(_before.Current.ShouldHaveSingleItem().Id);
    }

    internal async Task ExpectDatabaseAsync(Table table)
    {
        var expected = Values(table);
        Attach("Customer persistence expectations", expected);
        var after = await LoadAsync();
        Attach("Customer database after", JsonSerializer.SerializeToElement(after, EvidenceJson));
        _before.ShouldNotBeNull();
        _response.ShouldNotBeNull();
        _before.Current.Length.ShouldBe(Number(expected, "CurrentBefore"));
        _before.History.Length.ShouldBe(Number(expected, "HistoryBefore"));
        after.Current.Length.ShouldBe(Number(expected, "CurrentAfter"));
        after.History.Length.ShouldBe(Number(expected, "HistoryAfter"));
        var current = after.Current.ShouldHaveSingleItem();
        current.Id.ShouldBe(_response.Id);
        current.Version.ShouldBe(Number(expected, "VersionAfter"));
        AssertEqual(StoredProfile(current), StoredProfile(_response));
        if (Required(expected, "VersionBefore") == "none")
            _before.Current.ShouldBeEmpty();
        else
            _before.Current.ShouldHaveSingleItem().Version.ShouldBe(Number(expected, "VersionBefore"));

        // Retain each prior history document's complete projected data and metadata.
        foreach (var history in _before.History)
            AssertEqual(after.History.Single(item => item.Id == history.Id), history);

        var actions = Required(expected, "HistoryActions").Split(',').Select(Enum.Parse<ActionType>).ToArray();
        after.History.Select(item => item.Action).ShouldBe(actions);
        after.History.Select(item => item.Version).ShouldBe(Enumerable.Range(0, actions.Length).Select(value => (long)value));
        after.History.Select(item => item.Id).Distinct().Count().ShouldBe(after.History.Length);
        foreach (var history in after.History)
        {
            history.Id.ShouldNotBe(Guid.Empty);
            history.CustomerId.ShouldBe(current.Id);
            history.ChangedAt.ShouldNotBe(default);
            history.ChangedAt.Kind.ShouldBe(DateTimeKind.Utc);
        }
        var latest = after.History.Last();
        latest.Version.ShouldBe(current.Version);
        AssertEqual(StoredProfile(latest), StoredProfile(current));

        if (_operation == "Get")
        {
            AssertEqual(after, _before);
        }
        else
        {
            (after.History.Length - _before.History.Length).ShouldBe(1);
            InWindow(latest.ChangedAt);
            if (_before.Current.Length != 0)
            {
                current.Version.ShouldBe(_before.Current[0].Version + 1);
                AssertEqual(StoredProfile(_before.History.Last()), StoredProfile(_before.Current[0]));
            }
        }

        var timestamp = Required(expected, "UpdatedAt");
        if (timestamp == "generated")
            InWindow(current.UpdatedAt);
        else
        {
            timestamp.ShouldBe("retained");
            current.UpdatedAt.ShouldBe(_before.Current.ShouldHaveSingleItem().UpdatedAt);
        }

        if (_before.Current.Length != 0)
        {
            var previous = _before.Current[0];
            current.ExternalId.ShouldBe(previous.ExternalId);
            if (_operation != "UpdateIndividual")
                AssertEqual(current.Individual, previous.Individual);
            if (_operation == "UpdateIndividual")
                AssertEqual(current.Companies, previous.Companies);
            else if (_operation == "AddCompany")
            {
                foreach (var company in previous.Companies)
                    AssertEqual(current.Companies.Single(item => item.Id == company.Id), company);
                current.Companies.Count.ShouldBe(previous.Companies.Count + 1);
                current.Companies.Count(item => previous.Companies.All(old => old.Id != item.Id)).ShouldBe(1);
            }
            else if (_operation == "UpdateCompany")
            {
                current.Companies.Select(item => item.Id).Order().ShouldBe(previous.Companies.Select(item => item.Id).Order());
                current.Companies.Any(item => item.Id == TargetCompanyId).ShouldBeTrue();
                foreach (var company in previous.Companies.Where(item => item.Id != TargetCompanyId))
                    AssertEqual(current.Companies.Single(item => item.Id == company.Id), company);
            }
        }
        Attach("Customer persistence verified", new { operation = _operation, customerId = current.Id,
            current.Version, currentCount = after.Current.Length, historyCount = after.History.Length,
            previousHistoryPreserved = true, updatedAt = timestamp });
    }

    private async Task<Snapshot> LoadAsync()
    {
        var mongo = context.Factory.Services.GetRequiredService<MongoDbContext>();
        var current = await mongo.Customers.Find(FilterDefinition<CustomerDocument>.Empty).ToListAsync();
        var history = await mongo.CustomersHistory.Find(FilterDefinition<CustomersHistoryDocument>.Empty).ToListAsync();
        return new Snapshot(current.OrderBy(item => item.Id).ToArray(), history.OrderBy(item => item.Version).ThenBy(item => item.Id).ToArray());
    }

    private static int Number(IReadOnlyDictionary<string, string> expected, string key)
        => int.Parse(Required(expected, key), CultureInfo.InvariantCulture);

    private void InWindow(DateTime value)
    {
        // BSON dates store milliseconds; HTTP create/update responses may retain ticks.
        var actual = new DateTimeOffset(value).ToUnixTimeMilliseconds();
        actual.ShouldBeGreaterThanOrEqualTo(new DateTimeOffset(_started).ToUnixTimeMilliseconds());
        actual.ShouldBeLessThanOrEqualTo(new DateTimeOffset(_finished).ToUnixTimeMilliseconds());
    }

    private static void AssertProfile(CustomerResponseDto actual, CreateCustomerRequestDto expected)
    {
        actual.Id.ShouldNotBe(Guid.Empty);
        actual.UpdatedAt.ShouldNotBe(default);
        actual.UpdatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        actual.ExternalId.ShouldBe(expected.ExternalId);
        AssertEqual(actual.Individual, expected.Individual);
        actual.Companies.Count.ShouldBe(expected.Companies.Count);
        actual.Companies.Select(company => company.Id).Distinct().Count().ShouldBe(actual.Companies.Count);
        foreach (var company in expected.Companies)
        {
            var stored = actual.Companies.Single(item => item.TaxId == company.TaxId);
            stored.Id.ShouldNotBe(Guid.Empty);
            AssertEqual(new { stored.TaxId, stored.CompanyName, stored.BillingAddress, stored.ShippingAddress }, company);
        }
    }

    private static object StoredProfile(CustomerDocument customer)
        => new { customer.ExternalId, customer.Individual, customer.Companies, customer.UpdatedAt };
    private static object StoredProfile(CustomersHistoryDocument customer)
        => new { customer.ExternalId, customer.Individual, customer.Companies, customer.UpdatedAt };
    private static object StoredProfile(CustomerResponseDto customer)
        => new { customer.ExternalId, customer.Individual, customer.Companies,
            UpdatedAt = DateTimeOffset.FromUnixTimeMilliseconds(new DateTimeOffset(customer.UpdatedAt).ToUnixTimeMilliseconds()).UtcDateTime };

    private static void AssertEqual(object actual, object expected)
        => JsonElement.DeepEquals(JsonSerializer.SerializeToElement(actual, EvidenceJson),
            JsonSerializer.SerializeToElement(expected, EvidenceJson)).ShouldBeTrue("Complete customer data must match the explicit expectation.");

    private sealed record Snapshot(CustomerDocument[] Current, CustomersHistoryDocument[] History);
}

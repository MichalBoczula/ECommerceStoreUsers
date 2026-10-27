using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.RequestsDto.Favorites;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

/// <summary>Real HTTP framework rejections and complete MongoDB collection preservation.</summary>
public sealed class FrameworkFailureState(ScenarioApiContext context)
{
    private static readonly string[] Collections = ["customers", "customers-history", "admins", "admins-history", "favorites"];
    private static readonly JsonWriterSettings BsonJson = new() { OutputMode = JsonOutputMode.CanonicalExtendedJson };
    private Dictionary<string, JsonElement[]>? _before;
    private Dictionary<string, JsonElement[]>? _after;
    private string? _method;
    private string? _path;
    private string? _contentType;
    private byte[]? _bytes;
    private bool _hasContent;
    private Guid _customerId;
    private Guid _companyId;
    private Guid _adminId;

    internal async Task SeedAsync()
    {
        var address = new AddressRequestDto
        {
            PostalCode = "00-001", City = "Warsaw", Street = "Framework Street", BuildingNumber = "10", ApartmentNumber = null
        };
        var individual = new IndividualDataRequestDto
        {
            FirstName = "Original", LastName = "Framework", Email = "framework@example.com", Phone = "123456789",
            BillingAddress = address, ShippingAddress = address
        };
        var company = new AddCompanyRequestDto
        {
            TaxId = "1234567890", CompanyName = "Framework Company", BillingAddress = address, ShippingAddress = address
        };
        using var customerCreate = await context.HttpClient.PostAsJsonAsync("/customers", new CreateCustomerRequestDto
        {
            ExternalId = "framework-customer", Individual = individual, Companies = [company]
        }, context.JsonOptions);
        customerCreate.StatusCode.ShouldBe(HttpStatusCode.OK);
        var customer = await customerCreate.Content.ReadFromJsonAsync<CustomerResponseDto>(context.JsonOptions);
        customer.ShouldNotBeNull();
        _customerId = customer.Id;
        _companyId = customer.Companies.ShouldHaveSingleItem().Id;
        _customerId.ShouldNotBe(Guid.Empty);
        _companyId.ShouldNotBe(Guid.Empty);
        using var customerUpdate = await context.HttpClient.PutAsJsonAsync($"/customers/{_customerId}/individual",
            new UpdateIndividualDataRequestDto { Individual = individual with { FirstName = "Updated" } }, context.JsonOptions);
        customerUpdate.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var adminCreate = await context.HttpClient.PostAsJsonAsync("/admins", new CreateAdminRequestDto
        {
            ExternalId = "framework-admin", FullName = "Original Framework Admin", Email = "admin.framework@example.com"
        }, context.JsonOptions);
        adminCreate.StatusCode.ShouldBe(HttpStatusCode.OK);
        var admin = await adminCreate.Content.ReadFromJsonAsync<AdminResponseDto>(context.JsonOptions);
        admin.ShouldNotBeNull();
        _adminId = admin.Id;
        _adminId.ShouldNotBe(Guid.Empty);
        using var adminUpdate = await context.HttpClient.PutAsJsonAsync($"/admins/{_adminId}", new UpdateAdminProfileRequestDto
        {
            FullName = "Updated Framework Admin", Email = "admin.framework@example.com"
        }, context.JsonOptions);
        adminUpdate.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var favorite = await context.HttpClient.PostAsJsonAsync("/favorites/clients/77777777-7777-7777-7777-777777777777",
            new AddFavoriteRequestDto { ProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") }, context.JsonOptions);
        favorite.StatusCode.ShouldBe(HttpStatusCode.OK);
        var snapshot = await SnapshotAsync();
        snapshot["customers"].Length.ShouldBe(1);
        snapshot["customers-history"].Length.ShouldBe(2);
        snapshot["admins"].Length.ShouldBe(1);
        snapshot["admins-history"].Length.ShouldBe(2);
        snapshot["favorites"].Length.ShouldBe(1);
        // Both aggregate seeds include a real changed update and both history versions.
        foreach (var collection in new[] { "customers", "admins" })
            snapshot[collection][0].GetProperty("Version").GetProperty("$numberLong").GetString().ShouldBe("1");
        foreach (var collection in new[] { "customers-history", "admins-history" })
            snapshot[collection].Select(item => item.GetProperty("Version").GetProperty("$numberLong").GetString()).Order()
                .ShouldBe(new[] { "0", "1" });
        Attach("Framework checked HTTP setup", new { customerId = _customerId, companyId = _companyId, adminId = _adminId, collections = snapshot });
    }

    internal async Task BeginAsync()
    {
        _before = await SnapshotAsync();
        _after = null;
        Attach("Framework database before", _before);
    }

    internal void Prepare(Table table)
    {
        var values = Values(table);
        Prepare(Required(values, "Method"), Required(values, "Path"), Required(values, "Body"), Required(values, "ContentType"));
    }

    internal void Prepare(string method, string path, string body, string contentType)
    {
        _method = method;
        _path = Resolve(path);
        _hasContent = body != "<no-content>";
        _bytes = _hasContent && body != "<docstring>" ? Encoding.UTF8.GetBytes(body == "<empty>" ? string.Empty : body) : null;
        _contentType = contentType == "<none>" ? null : contentType;
    }

    internal void SetBody(string body) => _bytes = Encoding.UTF8.GetBytes(body);

    internal async Task SendAsync()
    {
        if (_before is null)
            await BeginAsync();
        _method.ShouldNotBeNull();
        _path.ShouldNotBeNull();
        if (_hasContent)
            _bytes.ShouldNotBeNull();
        Attach("Framework request expectations", new
        {
            method = _method, path = _path, contentType = _contentType, hasContent = _hasContent,
            byteLength = _bytes?.Length ?? 0, bodyBytesBase64 = _bytes is null ? null : Convert.ToBase64String(_bytes)
        });
        using var request = new HttpRequestMessage(new HttpMethod(_method), _path);
        if (_hasContent)
        {
            request.Content = new ByteArrayContent(_bytes!);
            if (_contentType is not null)
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(_contentType);
        }
        context.Response = await context.HttpClient.SendAsync(request);
        await CaptureAfterAsync();
        // Check the content bytes actually sent; do not reserialize invalid JSON.
        if (_hasContent)
            (await request.Content!.ReadAsByteArrayAsync()).ShouldBe(_bytes!);
        else
            request.Content.ShouldBeNull();
        Attach("Framework request bytes verified", new { method = _method, path = _path, exactBytes = true });
    }

    internal async Task ExpectProblemAsync(Table table)
    {
        var expected = Values(table);
        await ExpectProblemAsync(int.Parse(Required(expected, "Status"), CultureInfo.InvariantCulture),
            Required(expected, "Code"), Required(expected, "Title"), Required(expected, "Detail"),
            Required(expected, "MissingProperties"), Required(expected, "Allow"), expected.GetValueOrDefault("Errors", "[]"));
    }

    internal async Task ExpectProblemAsync(int status, string code, string title, string detail, string missing, string allow, string errors = "[]")
    {
        await CaptureAfterAsync();
        context.Response.ShouldNotBeNull();
        var path = context.Response.RequestMessage!.RequestUri!.AbsolutePath;
        if (_path is not null)
            path.ShouldBe(_path);
        var type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5." + (status switch
        {
            400 => "1", 404 => "4", 405 => "5", 415 => "13", _ => throw new ArgumentOutOfRangeException(nameof(status))
        });
        string[] missingProperties = missing == "[]" ? [] : missing.Split(',').Select(value => value.Trim()).ToArray();
        var expectedErrors = JsonSerializer.Deserialize<JsonElement>(errors);
        Attach("Framework problem expectations", new { status, type, title, detail, instance = path, code, errors = expectedErrors, missingProperties, traceId = "generated", allow });
        context.Response.StatusCode.ShouldBe((HttpStatusCode)status);
        context.Response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var raw = await context.Response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(raw);
        var problem = document.RootElement;
        problem.EnumerateObject().Select(item => item.Name).Order().ShouldBe(new[]
        {
            "code", "detail", "errors", "instance", "missingProperties", "status", "title", "traceId", "type"
        });
        problem.GetProperty("status").GetInt32().ShouldBe(status);
        problem.GetProperty("type").GetString().ShouldBe(type);
        problem.GetProperty("title").GetString().ShouldBe(title);
        problem.GetProperty("detail").GetString().ShouldBe(detail);
        problem.GetProperty("instance").GetString().ShouldBe(path);
        problem.GetProperty("code").GetString().ShouldBe(code);
        JsonElement.DeepEquals(problem.GetProperty("errors"), expectedErrors).ShouldBeTrue();
        problem.GetProperty("missingProperties").EnumerateArray().Select(item => item.GetString()).ShouldBe(missingProperties);
        problem.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
        raw.ShouldNotContain("INTERNAL_FAILURE_MARKER");
        raw.ShouldNotContain("stackTrace");
        raw.ShouldNotContain("System.Text.Json");
        var allowed = context.Response.Content.Headers.Allow.Order().ToArray();
        allowed.ShouldBe(allow == "[]" ? [] : allow.Split(',').Select(value => value.Trim()).Order().ToArray());
        Attach("Framework problem verified", new { problem = problem.Clone(), allowedMethods = allowed, completeSafeShape = true });
    }

    internal async Task ExpectUnchangedAsync()
    {
        await CaptureAfterAsync();
        _before.ShouldNotBeNull();
        _after.ShouldNotBeNull();
        JsonElement.DeepEquals(JsonSerializer.SerializeToElement(_after), JsonSerializer.SerializeToElement(_before))
            .ShouldBeTrue("Every field in all five current/history/Favorite collections must remain unchanged.");
        Attach("Framework persistence verified", new
        {
            unchanged = true, collections = Collections,
            counts = _after.ToDictionary(item => item.Key, item => item.Value.Length),
            effect = "No business write; not a rollback test"
        });
    }

    private async Task CaptureAfterAsync()
    {
        _before.ShouldNotBeNull();
        if (_after is null)
        {
            _after = await SnapshotAsync();
            Attach("Framework database after", _after);
        }
    }

    private async Task<Dictionary<string, JsonElement[]>> SnapshotAsync()
    {
        var database = AcceptanceMongoDb.GetDatabase(context.DatabaseName);
        var snapshot = new Dictionary<string, JsonElement[]>(StringComparer.Ordinal);
        foreach (var name in Collections)
        {
            var documents = await database.GetCollection<BsonDocument>(name).Find(FilterDefinition<BsonDocument>.Empty)
                .Sort(Builders<BsonDocument>.Sort.Ascending("_id")).ToListAsync();
            snapshot[name] = documents.Select(item => JsonSerializer.Deserialize<JsonElement>(item.ToJson(BsonJson))).ToArray();
        }
        return snapshot;
    }

    private string Resolve(string value) => value.Replace("{customerId}", _customerId.ToString(), StringComparison.Ordinal)
        .Replace("{companyId}", _companyId.ToString(), StringComparison.Ordinal).Replace("{adminId}", _adminId.ToString(), StringComparison.Ordinal);
    private void Attach(string name, object value) => context.RecordEvidence(name, value);
    private static Dictionary<string, string> Values(Table table)
        => table.Rows.ToDictionary(row => row["Field"], row => row["Value"], StringComparer.Ordinal);
    private static string Required(IReadOnlyDictionary<string, string> values, string field)
        => values.TryGetValue(field, out var value) ? value : throw new InvalidOperationException($"Missing explicit framework expectation: {field}");
}

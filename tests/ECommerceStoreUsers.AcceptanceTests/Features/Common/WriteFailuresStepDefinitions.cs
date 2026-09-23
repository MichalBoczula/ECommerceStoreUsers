using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class WriteFailuresStepDefinitions(ScenarioApiContext context) : IDisposable
{
    private const string SensitiveDetail = "REF07_INTERNAL_WRITE_FAILURE_DO_NOT_EXPOSE";
    private readonly string _externalId = $"ref07-{Guid.NewGuid():N}";
    private WebApplicationFactory<ECommerceStoreUsers.API.Program>? _factory;
    private HttpClient? _client;
    private string? _operation;
    private string? _path;
    private Guid _customerId;
    private Guid _companyId;
    private Guid _adminId;
    private string[] _collections = [];
    private string[] _before = [];
    private int _writeCalls;

    [Given("the {string} write will fail at the repository")]
    public async Task GivenTheWriteWillFail(string operationId)
    {
        _operation = operationId;
        var admin = operationId is "CreateAdmin" or "UpdateAdminProfile";
        _collections = admin ? ["admins", "admins-history"] : ["customers", "customers-history"];

        if (operationId is "UpdateIndividualData" or "AddCompany" or "UpdateCompany")
        {
            using var seed = await context.HttpClient.PostAsJsonAsync("/customers", CustomerRequest(
                operationId == "UpdateCompany" ? [CompanyRequest()] : []), context.JsonOptions);
            seed.EnsureSuccessStatusCode();
            var customer = await seed.Content.ReadFromJsonAsync<CustomerResponseDto>(context.JsonOptions);
            customer.ShouldNotBeNull();
            _customerId = customer.Id;
            if (operationId == "UpdateCompany")
            {
                _companyId = customer.Companies.Single().Id;
            }
        }
        else if (operationId == "UpdateAdminProfile")
        {
            using var seed = await context.HttpClient.PostAsJsonAsync("/admins", AdminRequest(), context.JsonOptions);
            seed.EnsureSuccessStatusCode();
            var adminResponse = await seed.Content.ReadFromJsonAsync<AdminResponseDto>(context.JsonOptions);
            adminResponse.ShouldNotBeNull();
            _adminId = adminResponse.Id;
        }

        _before = await Snapshot();
        _factory = context.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            if (admin)
            {
                var registration = services.Single(x => x.ServiceType == typeof(IAdminRepository));
                services.RemoveAll<IAdminRepository>();
                services.AddScoped<IAdminRepository>(provider => new FailingAdminRepository(
                    (IAdminRepository)ActivatorUtilities.CreateInstance(provider, registration.ImplementationType!),
                    () => _writeCalls++));
            }
            else
            {
                var registration = services.Single(x => x.ServiceType == typeof(ICustomerRepository));
                services.RemoveAll<ICustomerRepository>();
                services.AddScoped<ICustomerRepository>(provider => new FailingCustomerRepository(
                    (ICustomerRepository)ActivatorUtilities.CreateInstance(provider, registration.ImplementationType!),
                    () => _writeCalls++));
            }
        }));
        _client = _factory.CreateClient();
    }

    [When("I submit the failing {string} write")]
    public async Task WhenISubmitTheFailingWrite(string operationId)
    {
        operationId.ShouldBe(_operation);
        _client.ShouldNotBeNull();
        _path = operationId switch
        {
            "CreateCustomer" => "/customers",
            "UpdateIndividualData" => $"/customers/{_customerId}/individual",
            "AddCompany" => $"/customers/{_customerId}/companies",
            "UpdateCompany" => $"/customers/{_customerId}/companies/{_companyId}",
            "CreateAdmin" => "/admins",
            "UpdateAdminProfile" => $"/admins/{_adminId}",
            _ => throw new ArgumentOutOfRangeException(nameof(operationId))
        };
        context.Response = operationId switch
        {
            "CreateCustomer" => await _client.PostAsJsonAsync(_path, CustomerRequest([]), context.JsonOptions),
            "UpdateIndividualData" => await _client.PutAsJsonAsync(_path,
                new UpdateIndividualDataRequestDto { Individual = IndividualRequest("Updated") }, context.JsonOptions),
            "AddCompany" => await _client.PostAsJsonAsync(_path, CompanyRequest(), context.JsonOptions),
            "UpdateCompany" => await _client.PutAsJsonAsync(_path, new UpdateCompanyRequestDto
            {
                TaxId = "9876543210", CompanyName = "Updated Company",
                BillingAddress = AddressRequest(), ShippingAddress = AddressRequest()
            }, context.JsonOptions),
            "CreateAdmin" => await _client.PostAsJsonAsync(_path, AdminRequest(), context.JsonOptions),
            "UpdateAdminProfile" => await _client.PutAsJsonAsync(_path,
                new UpdateAdminProfileRequestDto { FullName = "Updated Admin", Email = "updated@example.com" },
                context.JsonOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(operationId))
        };
    }

    [Then("the write reports a safe 500 and leaves current and history unchanged")]
    public async Task ThenTheWriteFailsWithoutChanges()
    {
        context.Response.ShouldNotBeNull();
        _writeCalls.ShouldBe(1);
        context.Response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        context.Response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await context.Response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("status").GetInt32().ShouldBe(500);
        json.RootElement.GetProperty("code").GetString().ShouldBe("internal_error");
        json.RootElement.GetProperty("instance").GetString().ShouldBe(_path);
        json.RootElement.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
        body.ShouldNotContain(SensitiveDetail);
        body.ShouldNotContain("stackTrace");
        (await Snapshot()).ShouldBe(_before);
    }

    private async Task<string[]> Snapshot()
    {
        var database = AcceptanceMongoDb.GetDatabase(context.DatabaseName);
        var snapshots = new List<string>();
        foreach (var name in _collections)
        {
            var documents = await database.GetCollection<BsonDocument>(name)
                .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
            snapshots.Add(JsonSerializer.Serialize(documents.Select(doc => doc.ToJson()).OrderBy(doc => doc)));
        }
        return [.. snapshots];
    }

    private CreateCustomerRequestDto CustomerRequest(IReadOnlyCollection<AddCompanyRequestDto> companies) => new()
    {
        ExternalId = _externalId,
        Individual = IndividualRequest("Original"),
        Companies = companies
    };

    private static IndividualDataRequestDto IndividualRequest(string firstName) => new()
    {
        FirstName = firstName, LastName = "Acceptance", Email = "ref07@example.com", Phone = "123456789",
        BillingAddress = AddressRequest(), ShippingAddress = AddressRequest()
    };

    private static AddressRequestDto AddressRequest() => new()
    {
        PostalCode = "00-001", City = "Warsaw", Street = "Main Street",
        BuildingNumber = "10", ApartmentNumber = "1"
    };

    private static AddCompanyRequestDto CompanyRequest() => new()
    {
        TaxId = "1234567890", CompanyName = "Acceptance Company",
        BillingAddress = AddressRequest(), ShippingAddress = AddressRequest()
    };

    private CreateAdminRequestDto AdminRequest() => new()
    {
        ExternalId = _externalId, FullName = "Acceptance Admin", Email = "ref07@example.com"
    };

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    private sealed class FailingCustomerRepository(ICustomerRepository inner, Action onWrite) : ICustomerRepository
    {
        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct) => inner.GetByIdAsync(id, ct);
        public Task<Customer?> GetByExternalIdAsync(string id, CancellationToken ct) => inner.GetByExternalIdAsync(id, ct);
        public Task<Customer> CreateCustomer(Customer customer, CancellationToken ct) => Fail();
        public Task<Customer> UpdateCustomer(Customer customer, CancellationToken ct) => Fail();
        private Task<Customer> Fail()
        {
            onWrite();
            return Task.FromException<Customer>(new InvalidOperationException(SensitiveDetail));
        }
    }

    private sealed class FailingAdminRepository(IAdminRepository inner, Action onWrite) : IAdminRepository
    {
        public Task<Admin?> GetByIdAsync(Guid id, CancellationToken ct) => inner.GetByIdAsync(id, ct);
        public Task<Admin?> GetByExternalIdAsync(string id, CancellationToken ct) => inner.GetByExternalIdAsync(id, ct);
        public Task<Admin> CreateAdmin(Admin admin, CancellationToken ct) => Fail();
        public Task<Admin> UpdateAdmin(Admin admin, CancellationToken ct) => Fail();
        private Task<Admin> Fail()
        {
            onWrite();
            return Task.FromException<Admin>(new InvalidOperationException(SensitiveDetail));
        }
    }
}

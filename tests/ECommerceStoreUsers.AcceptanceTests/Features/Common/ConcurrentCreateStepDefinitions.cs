using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using Reqnroll;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class ConcurrentCreateStepDefinitions(ScenarioApiContext context, AtomicityState atomicity) : IDisposable
{
    private WebApplicationFactory<ECommerceStoreUsers.API.Program>? _factory;
    private readonly TaskCompletionSource _bothLoaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _reads;
    private string _operation = string.Empty;
    private object[] _requests = [];
    private HttpResponseMessage[] _responses = [];
    private DateTime _started;
    private DateTime _finished;

    [Given("two valid {string} creates share one external identifier")]
    public async Task GivenContenders(string operation, Table table)
    {
        _operation = operation;
        var values = table.Rows.ToDictionary(row => row["Field"], row => new[] { row["First"], row["Second"] });
        _requests = Enumerable.Range(0, 2).Select(index => Request(operation, values, index)).ToArray();
        context.RecordEvidence("Concurrent create explicit requests", new { operation, requests = _requests });
        await new FrameworkFailureState(context).SeedAsync();
        await atomicity.BeginAsync();
        _factory = context.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            if (operation == "CreateCustomer")
            {
                services.RemoveAll<ICustomerRepository>();
                services.AddScoped<ICustomerRepository>(provider => new CustomerGate(
                    new CustomerRepository(provider.GetRequiredService<MongoDbContext>()), ReadTogether));
            }
            else
            {
                operation.ShouldBe("CreateAdmin");
                services.RemoveAll<IAdminRepository>();
                services.AddScoped<IAdminRepository>(provider => new AdminGate(
                    new AdminRepository(provider.GetRequiredService<MongoDbContext>()), ReadTogether));
            }
        }));
        context.HttpClient.Dispose();
        context.HttpClient = context.CreateClient(_factory);
    }

    private async Task ReadTogether(CancellationToken token)
    {
        if (Interlocked.Increment(ref _reads) == 2)
            _bothLoaded.SetResult();
        // Only a deadlock guard; assertions do not depend on timing or a particular winner.
        await _bothLoaded.Task.WaitAsync(TimeSpan.FromSeconds(30), token);
    }

    [When("both create requests are submitted concurrently")]
    public async Task WhenBothCreate()
    {
        var path = _operation == "CreateCustomer" ? "/customers" : "/admins";
        _started = DateTime.UtcNow;
        _responses = await Task.WhenAll(_requests.Select(request => context.HttpClient.PostAsJsonAsync(path, request, context.JsonOptions)));
        _finished = DateTime.UtcNow;
    }

    [Then("one create succeeds and the other returns the complete 409 conflict")]
    public async Task ThenOneWinner()
    {
        _reads.ShouldBe(2);
        _responses.Select(response => (int)response.StatusCode).Order().ShouldBe(new[] { 200, 409 });
        var winnerIndex = Array.FindIndex(_responses, response => response.StatusCode == HttpStatusCode.OK);
        var response = _responses[winnerIndex];
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        var entity = _operation == "CreateCustomer" ? "Customer" : "Admin";
        BsonDocument expected;
        if (_requests[winnerIndex] is CreateCustomerRequestDto request)
        {
            var winner = JsonSerializer.Deserialize<CustomerResponseDto>(await response.Content.ReadAsStringAsync(), context.JsonOptions)!;
            winner.Id.ShouldNotBe(Guid.Empty);
            InWindow(winner.UpdatedAt);
            var companyId = winner.Companies.ShouldHaveSingleItem().Id;
            companyId.ShouldNotBe(Guid.Empty);
            var company = (request.Companies ?? throw new InvalidOperationException("The Customer race fixture requires a company.")).Single();
            var individual = request.Individual;
            var expectedCustomer = Customer.Rehydrate(winner.Id, request.ExternalId,
                new IndividualData(individual.FirstName, individual.LastName, individual.Email, individual.Phone,
                    Address(individual.BillingAddress), Address(individual.ShippingAddress)),
                [CompanyData.Rehydrate(companyId, company.TaxId, company.CompanyName,
                    Address(company.BillingAddress), Address(company.ShippingAddress))], winner.UpdatedAt);
            var expectedResponse = new
            {
                winner.Id,
                request.ExternalId,
                request.Individual,
                Companies = new[] { new { Id = companyId, company.TaxId, company.CompanyName, company.BillingAddress, company.ShippingAddress } },
                winner.UpdatedAt
            };
            JsonElement.DeepEquals(JsonSerializer.SerializeToElement(winner, context.JsonOptions), JsonSerializer.SerializeToElement(expectedResponse, context.JsonOptions)).ShouldBeTrue();
            expected = CustomerMapping.MapToDocument(expectedCustomer).ToBsonDocument();
        }
        else
        {
            var adminRequest = (CreateAdminRequestDto)_requests[winnerIndex];
            var winner = JsonSerializer.Deserialize<AdminResponseDto>(await response.Content.ReadAsStringAsync(), context.JsonOptions)!;
            winner.Id.ShouldNotBe(Guid.Empty);
            InWindow(winner.LastLoginAt);
            var admin = Admin.Rehydrate(winner.Id, adminRequest.ExternalId, adminRequest.FullName, adminRequest.Email, true, winner.LastLoginAt);
            var expectedResponse = new { winner.Id, adminRequest.ExternalId, adminRequest.FullName, adminRequest.Email, IsActive = true, winner.LastLoginAt };
            JsonElement.DeepEquals(JsonSerializer.SerializeToElement(winner, context.JsonOptions), JsonSerializer.SerializeToElement(expectedResponse, context.JsonOptions)).ShouldBeTrue();
            expected = AdminMapping.MapToDocument(admin).ToBsonDocument();
        }
        await atomicity.CommitAggregateAsync(entity == "Customer" ? "customers" : "admins", expected);
        context.Response = _responses[1 - winnerIndex];
        using var problem = JsonDocument.Parse(await context.Response.Content.ReadAsStringAsync());
        var code = problem.RootElement.GetProperty("code").GetString();
        code.ShouldBeOneOf("resource_conflict", "concurrency_conflict");
        await atomicity.ProblemAsync(409, code!, "Conflict.", code == "resource_conflict"
            ? $"Resource {entity} identified by id ref08-concurrent already exists in db. Error in action {_operation}."
            : "The resource was changed by another request. Reload it and retry.");
        context.RecordEvidence("Concurrent create result verified", new { operation = _operation, winnerIndex, losingCode = code, bothInitialLookupsEmpty = true, repositoryLookups = _reads });
    }

    private void InWindow(DateTime value)
    {
        value.Kind.ShouldBe(DateTimeKind.Utc);
        value.ShouldBeGreaterThanOrEqualTo(_started);
        value.ShouldBeLessThanOrEqualTo(_finished);
    }

    private static object Request(string operation, Dictionary<string, string[]> values, int index)
    {
        string Value(string field) => values[field][index];
        Value("ExternalId").ShouldBe("ref08-concurrent");
        if (operation == "CreateAdmin")
            return new CreateAdminRequestDto { ExternalId = Value("ExternalId"), FullName = Value("Name") + " Admin", Email = Value("Email") };
        var address = new AddressRequestDto
        {
            PostalCode = "00-001",
            City = Value("City"),
            Street = Value("Name") + " Street",
            BuildingNumber = "10",
            ApartmentNumber = null
        };
        return new CreateCustomerRequestDto
        {
            ExternalId = Value("ExternalId"),
            Individual = new IndividualDataRequestDto
            {
                FirstName = Value("Name"),
                LastName = "Person",
                Email = Value("Email"),
                Phone = "123456789",
                BillingAddress = address,
                ShippingAddress = address
            },
            Companies = [new AddCompanyRequestDto { TaxId = Value("TaxId"), CompanyName = Value("Name") + " Company", BillingAddress = address, ShippingAddress = address }]
        };
    }

    private static Address Address(AddressRequestDto address)
        => new(address.PostalCode, address.City, address.Street, address.BuildingNumber, address.ApartmentNumber);

    public void Dispose()
    {
        foreach (var response in _responses)
            if (!ReferenceEquals(response, context.Response))
                response.Dispose();
        _factory?.Dispose();
    }

    private sealed class CustomerGate(CustomerRepository inner, Func<CancellationToken, Task> gate) : ICustomerRepository
    {
        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken token) => inner.GetByIdAsync(id, token);
        public async Task<Customer?> GetByExternalIdAsync(string externalId, CancellationToken token)
        {
            var result = await inner.GetByExternalIdAsync(externalId, token);
            result.ShouldBeNull();
            await gate(token);
            return result;
        }
        public Task<Customer> CreateCustomer(Customer customer, CancellationToken token) => inner.CreateCustomer(customer, token);
        public Task<Customer> UpdateCustomer(Customer customer, CancellationToken token) => inner.UpdateCustomer(customer, token);
    }

    private sealed class AdminGate(AdminRepository inner, Func<CancellationToken, Task> gate) : IAdminRepository
    {
        public Task<Admin?> GetByIdAsync(Guid id, CancellationToken token) => inner.GetByIdAsync(id, token);
        public async Task<Admin?> GetByExternalIdAsync(string externalId, CancellationToken token)
        {
            var result = await inner.GetByExternalIdAsync(externalId, token);
            result.ShouldBeNull();
            await gate(token);
            return result;
        }
        public Task<Admin> CreateAdmin(Admin admin, CancellationToken token) => inner.CreateAdmin(admin, token);
        public Task<Admin> UpdateAdmin(Admin admin, CancellationToken token) => inner.UpdateAdmin(admin, token);
    }
}

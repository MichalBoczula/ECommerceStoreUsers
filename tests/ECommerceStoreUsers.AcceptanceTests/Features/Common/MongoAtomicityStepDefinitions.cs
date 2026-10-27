using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Repositories;
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

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class MongoAtomicityStepDefinitions(ScenarioApiContext context, AtomicityState atomicity) : IDisposable
{
    private const string ExternalId = "ref08-target";
    private WebApplicationFactory<ECommerceStoreUsers.API.Program>? _factory;
    private Guid _id;
    private Guid _companyId;
    private string _operation = string.Empty;

    [Given("the {string} create loses an external identifier unique key race")]
    public async Task GivenCreateRace(string operation)
    {
        _operation = operation;
        await new FrameworkFailureState(context).SeedAsync();
        await atomicity.BeginAsync();
        UseRepository(true);
    }

    [Given("the {string} transaction will fail inserting history in MongoDB")]
    public async Task GivenHistoryFailure(string operation)
    {
        _operation = operation;
        await new FrameworkFailureState(context).SeedAsync();
        var mongo = context.Factory.Services.GetRequiredService<MongoDbContext>();
        var admin = operation is "CreateAdmin" or "UpdateAdminProfile";
        if (operation == "CreateCustomer")
            await mongo.CustomersHistory.InsertOneAsync(CustomerMapping.MapToHistoryDocument(Customer(), ActionType.Insert));
        else if (operation == "CreateAdmin")
            await mongo.AdminsHistory.InsertOneAsync(AdminMapping.MapToHistoryDocument(Admin(), ActionType.Insert));
        else if (admin)
        {
            using var response = await context.HttpClient.PostAsJsonAsync("/admins", AdminRequest(), context.JsonOptions);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            _id = (await response.Content.ReadFromJsonAsync<AdminResponseDto>(context.JsonOptions))!.Id;
        }
        else
        {
            using var response = await context.HttpClient.PostAsJsonAsync("/customers", CustomerRequest(), context.JsonOptions);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var customer = (await response.Content.ReadFromJsonAsync<CustomerResponseDto>(context.JsonOptions))!;
            _id = customer.Id;
            _companyId = customer.Companies.First().Id;
        }
        var collection = admin ? "admins-history" : "customers-history";
        await AcceptanceMongoDb.GetDatabase(context.DatabaseName).GetCollection<BsonDocument>(collection).Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("ExternalId"),
                new CreateIndexOptions<BsonDocument>
                {
                    Name = "ref08_history_failure", Unique = true,
                    PartialFilterExpression = Builders<BsonDocument>.Filter.Eq("ExternalId", ExternalId)
                }));
        context.RecordEvidence("Atomicity history failure arrangement", new { collection, index = "ref08_history_failure", field = "ExternalId", value = ExternalId, targetOnly = true });
        await atomicity.BeginAsync();
        UseRepository(false);
    }

    private void UseRepository(bool race)
    {
        _factory = context.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            if (_operation is "CreateAdmin" or "UpdateAdminProfile")
            {
                services.RemoveAll<IAdminRepository>();
                services.AddScoped<IAdminRepository>(provider => new ObservedAdminRepository(
                    new AdminRepository(provider.GetRequiredService<MongoDbContext>()), race, atomicity));
            }
            else
            {
                services.RemoveAll<ICustomerRepository>();
                services.AddScoped<ICustomerRepository>(provider => new ObservedCustomerRepository(
                    new CustomerRepository(provider.GetRequiredService<MongoDbContext>()), race, atomicity));
            }
        }));
        context.HttpClient.Dispose();
        context.HttpClient = context.CreateClient(_factory);
    }

    [When("I submit the observed {string} MongoDB write")]
    public async Task WhenWrite(string operation)
    {
        operation.ShouldBe(_operation);
        context.Response = operation switch
        {
            "CreateCustomer" => await context.HttpClient.PostAsJsonAsync("/customers", CustomerRequest(), context.JsonOptions),
            "UpdateIndividualData" => await context.HttpClient.PutAsJsonAsync($"/customers/{_id}/individual",
                new UpdateIndividualDataRequestDto { Individual = Individual() with { FirstName = "Attempt" } }, context.JsonOptions),
            "AddCompany" => await context.HttpClient.PostAsJsonAsync($"/customers/{_id}/companies", Company() with { TaxId = "9876543210" }, context.JsonOptions),
            "UpdateCompany" => await context.HttpClient.PutAsJsonAsync($"/customers/{_id}/companies/{_companyId}", new UpdateCompanyRequestDto
            {
                TaxId = "9876543210", CompanyName = "Attempt Company", BillingAddress = Address(), ShippingAddress = Address()
            }, context.JsonOptions),
            "CreateAdmin" => await context.HttpClient.PostAsJsonAsync("/admins", AdminRequest(), context.JsonOptions),
            "UpdateAdminProfile" => await context.HttpClient.PutAsJsonAsync($"/admins/{_id}", new UpdateAdminProfileRequestDto
            {
                FullName = "Attempt Admin", Email = "attempt@example.com"
            }, context.JsonOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
    }

    [Then("the real MongoDB duplicate key error is safely reported as 500")]
    public async Task ThenMongoFailure()
    {
        atomicity.ExpectMongoFailure();
        await atomicity.ProblemAsync(500, "internal_error", "Server error.", "An unexpected error occurred.");
    }

    [Then("only the committed race or rollback outcome remains")]
    public Task ThenOutcome(Table table) => atomicity.ExpectAsync(table);

    private static AddressRequestDto Address() => new()
    {
        PostalCode = "00-001", City = "Warsaw", Street = "Target Street", BuildingNumber = "10", ApartmentNumber = null
    };
    private static IndividualDataRequestDto Individual() => new()
    {
        FirstName = "Target", LastName = "Person", Email = "target@example.com", Phone = "123456789",
        BillingAddress = Address(), ShippingAddress = Address()
    };
    private static AddCompanyRequestDto Company() => new()
    {
        TaxId = "1234567890", CompanyName = "Target Company", BillingAddress = Address(), ShippingAddress = Address()
    };
    private static CreateCustomerRequestDto CustomerRequest() => new() { ExternalId = ExternalId, Individual = Individual(), Companies = [Company()] };
    private static CreateAdminRequestDto AdminRequest() => new() { ExternalId = ExternalId, FullName = "Target Admin", Email = "target.admin@example.com" };
    private static Customer Customer()
    {
        var address = new Address("11-222", "Gdansk", "Winner Street", "23", "4");
        var customer = new Customer(ExternalId, new IndividualData("Winner", "Complete", "winner@example.com", "987654321", address, address));
        customer.AddCompany("Winner Company", "0987654321", address, address);
        return customer;
    }
    private static Admin Admin() => new(ExternalId, "Winner Admin", "winner.admin@example.com");

    public void Dispose() => _factory?.Dispose();

    private sealed class ObservedCustomerRepository(CustomerRepository inner, bool race, AtomicityState atomicity) : ICustomerRepository
    {
        private bool _triggered;
        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken token) => inner.GetByIdAsync(id, token);
        public async Task<Customer?> GetByExternalIdAsync(string externalId, CancellationToken token)
        {
            var loaded = await inner.GetByExternalIdAsync(externalId, token);
            if (race && externalId == ExternalId && loaded is null && !_triggered)
            {
                _triggered = true;
                var winner = Customer();
                await inner.CreateCustomer(winner, token);
                await atomicity.CommitAggregateAsync("customers", CustomerMapping.MapToDocument(winner).ToBsonDocument());
            }
            return loaded;
        }
        public Task<Customer> CreateCustomer(Customer customer, CancellationToken token) => Observe(() => inner.CreateCustomer(customer, token));
        public Task<Customer> UpdateCustomer(Customer customer, CancellationToken token) => Observe(() => inner.UpdateCustomer(customer, token));
        private async Task<Customer> Observe(Func<Task<Customer>> write)
        {
            try { return await write(); }
            catch (MongoWriteException error)
            {
                atomicity.RecordMongoFailure(error, race ? "ExternalId" : "ref08_history_failure", race ? "customers" : "customers-history");
                throw;
            }
        }
    }

    private sealed class ObservedAdminRepository(AdminRepository inner, bool race, AtomicityState atomicity) : IAdminRepository
    {
        private bool _triggered;
        public Task<Admin?> GetByIdAsync(Guid id, CancellationToken token) => inner.GetByIdAsync(id, token);
        public async Task<Admin?> GetByExternalIdAsync(string externalId, CancellationToken token)
        {
            var loaded = await inner.GetByExternalIdAsync(externalId, token);
            if (race && externalId == ExternalId && loaded is null && !_triggered)
            {
                _triggered = true;
                var winner = Admin();
                await inner.CreateAdmin(winner, token);
                await atomicity.CommitAggregateAsync("admins", AdminMapping.MapToDocument(winner).ToBsonDocument());
            }
            return loaded;
        }
        public Task<Admin> CreateAdmin(Admin admin, CancellationToken token) => Observe(() => inner.CreateAdmin(admin, token));
        public Task<Admin> UpdateAdmin(Admin admin, CancellationToken token) => Observe(() => inner.UpdateAdmin(admin, token));
        private async Task<Admin> Observe(Func<Task<Admin>> write)
        {
            try { return await write(); }
            catch (MongoWriteException error)
            {
                atomicity.RecordMongoFailure(error, race ? "ExternalId" : "ref08_history_failure", race ? "admins" : "admins-history");
                throw;
            }
        }
    }
}

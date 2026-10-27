using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
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

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers;

[Binding]
public sealed class CustomerWriteRacesStepDefinitions(ScenarioApiContext context, AtomicityState atomicity) : IDisposable
{
    private WebApplicationFactory<ECommerceStoreUsers.API.Program>? _factory;
    private Guid _id;
    private bool _delete;
    private string _operation = "UpdateIndividualData";
    private Guid _companyId;

    [Given("a customer update loses a concurrent change race")]
    public Task GivenChangeRace() => Arrange(false);

    [Given("a customer update loses a concurrent delete race")]
    public Task GivenDeleteRace() => Arrange(true);

    [Given("a {string} write loses a {string} company race")]
    public Task GivenCompanyRace(string operation, string race) => Arrange(race == "Delete", operation, race == "SameTaxId");

    private async Task Arrange(bool delete, string operation = "UpdateIndividualData", bool sameTaxId = false)
    {
        _delete = delete;
        _operation = operation;
        using var response = await context.HttpClient.PostAsJsonAsync("/customers", new CreateCustomerRequestDto
        {
            ExternalId = $"customer-race-{Guid.NewGuid():N}",
            Individual = Individual("Before"),
            Companies = [Company("Original One", "1234567890"), Company("Original Two", "0987654321")]
        }, context.JsonOptions);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var customer = (await response.Content.ReadFromJsonAsync<CustomerResponseDto>(context.JsonOptions))!;
        _id = customer.Id;
        _companyId = customer.Companies.First().Id;
        await atomicity.BeginAsync();

        _factory = context.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICustomerRepository>();
            services.AddScoped<ICustomerRepository>(provider =>
                new RacingRepository(new CustomerRepository(provider.GetRequiredService<MongoDbContext>()),
                    provider.GetRequiredService<MongoDbContext>(), delete, operation, sameTaxId, atomicity));
        }));
        context.HttpClient.Dispose();
        context.HttpClient = context.CreateClient(_factory);
    }

    [When("the raced individual update is submitted")]
    public async Task WhenUpdate()
    {
        context.Response = _operation switch
        {
            "UpdateIndividualData" => await context.HttpClient.PutAsJsonAsync($"/customers/{_id}/individual",
                new UpdateIndividualDataRequestDto { Individual = Individual("Attempt") }, context.JsonOptions),
            "AddCompany" => await context.HttpClient.PostAsJsonAsync($"/customers/{_id}/companies", Company("Attempt Company", "9876543210"), context.JsonOptions),
            "UpdateCompany" => await context.HttpClient.PutAsJsonAsync($"/customers/{_id}/companies/{_companyId}", new UpdateCompanyRequestDto
            {
                TaxId = "9876543210", CompanyName = "Attempt Company", BillingAddress = Address(), ShippingAddress = Address()
            }, context.JsonOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(_operation))
        };
    }

    [Then("the raced update returns {int} with no additional history")]
    public async Task ThenStatusAndHistory(int status)
    {
        await atomicity.ProblemAsync(status, _delete ? "resource_not_found" : "concurrency_conflict",
            _delete ? "Resource not found." : "Conflict.", _delete
                ? $"Resource Customer identified by id {_id} cannot be found in database during action UpdateCustomer."
                : "The resource was changed by another request. Reload it and retry.");
    }

    public void Dispose() => _factory?.Dispose();

    private static IndividualDataRequestDto Individual(string firstName) => new()
    {
        FirstName = firstName,
        LastName = "Person",
        Email = "person@example.com",
        Phone = "123456789",
        BillingAddress = Address(),
        ShippingAddress = Address()
    };

    private static AddressRequestDto Address() => new()
    {
        PostalCode = "00-001",
        City = "Warsaw",
        Street = "Street",
        BuildingNumber = "10",
        ApartmentNumber = "2"
    };

    private static AddCompanyRequestDto Company(string name, string taxId) => new()
    {
        CompanyName = name, TaxId = taxId, BillingAddress = Address(), ShippingAddress = Address()
    };

    private sealed class RacingRepository(CustomerRepository inner, MongoDbContext mongo, bool delete,
        string operation, bool sameTaxId, AtomicityState atomicity) : ICustomerRepository
    {
        private bool _triggered;

        public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken token)
        {
            var loaded = await inner.GetByIdAsync(id, token);
            if (loaded is not null && !_triggered)
            {
                _triggered = true;
                if (delete)
                {
                    (await mongo.Customers.DeleteOneAsync(x => x.Id == id, token)).DeletedCount.ShouldBe(1);
                    await atomicity.CommitDeleteAsync("customers", new BsonBinaryData(id, GuidRepresentation.Standard));
                }
                else
                {
                    var winner = await inner.GetByIdAsync(id, token);
                    winner.ShouldNotBeNull();
                    var billing = new Address("01-002", "Krakow", "Winner Billing", "21", null);
                    var shipping = new Address("02-003", "Gdansk", "Winner Shipping", "32", "4");
                    if (operation == "AddCompany")
                        winner.AddCompany("Winner Company", sameTaxId ? "9876543210" : "1111111111", billing, shipping);
                    else if (operation == "UpdateCompany")
                        winner.Companies.First().UpdateCompanyDetails(sameTaxId ? "9876543210" : "1111111111", "Winner Company", billing, shipping);
                    else
                        winner.UpdateIndividualData(new IndividualData("Winner", "Complete", "winner@example.com", "987654321", billing, shipping));
                    await inner.UpdateCustomer(winner, token);
                    await atomicity.CommitAggregateAsync("customers", CustomerMapping.MapToDocument(winner).ToBsonDocument());
                }
            }

            return loaded;
        }

        public Task<Customer?> GetByExternalIdAsync(string externalId, CancellationToken token)
            => inner.GetByExternalIdAsync(externalId, token);
        public Task<Customer> CreateCustomer(Customer customer, CancellationToken token)
            => inner.CreateCustomer(customer, token);
        public Task<Customer> UpdateCustomer(Customer customer, CancellationToken token)
            => inner.UpdateCustomer(customer, token);
    }
}


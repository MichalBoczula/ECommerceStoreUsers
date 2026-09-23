using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers;

[Binding]
public sealed class CustomerWriteRacesStepDefinitions(ScenarioApiContext context) : IDisposable
{
    private WebApplicationFactory<ECommerceStoreUsers.API.Program>? _factory;
    private Guid _id;
    private bool _delete;

    [Given("a customer update loses a concurrent change race")]
    public Task GivenChangeRace() => Arrange(false);

    [Given("a customer update loses a concurrent delete race")]
    public Task GivenDeleteRace() => Arrange(true);

    private async Task Arrange(bool delete)
    {
        _delete = delete;
        using var response = await context.HttpClient.PostAsJsonAsync("/customers", new CreateCustomerRequestDto
        {
            ExternalId = $"customer-race-{Guid.NewGuid():N}",
            Individual = Individual("Before")
        }, context.JsonOptions);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _id = (await response.Content.ReadFromJsonAsync<CustomerResponseDto>(context.JsonOptions))!.Id;

        _factory = context.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICustomerRepository>();
            services.AddScoped<ICustomerRepository>(provider =>
                new RacingRepository(new CustomerRepository(provider.GetRequiredService<MongoDbContext>()),
                    provider.GetRequiredService<MongoDbContext>(), delete));
        }));
        context.HttpClient.Dispose();
        context.HttpClient = _factory.CreateClient();
    }

    [When("the raced individual update is submitted")]
    public async Task WhenUpdate()
    {
        context.Response = await context.HttpClient.PutAsJsonAsync($"/customers/{_id}/individual",
            new UpdateIndividualDataRequestDto { Individual = Individual("Attempt") }, context.JsonOptions);
    }

    [Then("the raced update returns {int} with no additional history")]
    public async Task ThenStatusAndHistory(int status)
    {
        context.Response.ShouldNotBeNull();
        context.Response.StatusCode.ShouldBe((HttpStatusCode)status);
        context.Response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await context.Response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(context.JsonOptions);
        body.GetProperty("code").GetString().ShouldBe(_delete ? "resource_not_found" : "concurrency_conflict");
        var mongo = context.Factory.Services.GetRequiredService<MongoDbContext>();
        (await mongo.CustomersHistory.CountDocumentsAsync(x => x.CustomerId == _id)).ShouldBe(_delete ? 1 : 2);
        var persisted = await mongo.Customers.Find(x => x.Id == _id).FirstOrDefaultAsync();
        if (_delete)
            persisted.ShouldBeNull();
        else
        {
            persisted.ShouldNotBeNull();
            persisted.Individual.FirstName.ShouldBe("Winner");
            persisted.Version.ShouldBe(1);
        }
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

    private sealed class RacingRepository(CustomerRepository inner, MongoDbContext mongo, bool delete) : ICustomerRepository
    {
        private bool _triggered;

        public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken token)
        {
            var loaded = await inner.GetByIdAsync(id, token);
            if (loaded is not null && !_triggered)
            {
                _triggered = true;
                if (delete)
                    await mongo.Customers.DeleteOneAsync(x => x.Id == id, token);
                else
                {
                    var winner = await inner.GetByIdAsync(id, token);
                    winner!.UpdateIndividualData(new IndividualData("Winner", "Person", "person@example.com",
                        "123456789", new Address("00-001", "Warsaw", "Street", "10", "2"),
                        new Address("00-001", "Warsaw", "Street", "10", "2")));
                    await inner.UpdateCustomer(winner, token);
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

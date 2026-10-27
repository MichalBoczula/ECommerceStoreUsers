using ECommerceStoreUsers.Domain.Validation.Common;
using Xunit.Abstractions;
using ECommerceStoreUsers.Infrastructure.Configuration;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Tests;

public sealed class UniqueCreateRaceTests(MongoDbTestFixture fixture, ITestOutputHelper output) : IClassFixture<MongoDbTestFixture>
{
    [Fact]
    public async Task ConcurrentCustomerCreatesPreserveOnlyTheCompleteWinnerAndItsHistory()
    {
        await using var services = TestServiceProviderFactory.Create(fixture.ConnectionString, $"customer-create-race-{Guid.NewGuid():N}");
        await services.InitializeInfrastructureAsync();
        var repository = services.GetRequiredService<ICustomerRepository>();
        var context = services.GetRequiredService<MongoDbContext>();
        var address = new Address("00-001", "Warsaw", "Race Street", "10", null);
        Customer Contender(string name, string taxId)
        {
            var customer = new Customer("shared-external-id", new IndividualData(name, "Person", name.ToLowerInvariant() + "@example.com", "123456789", address, address));
            customer.AddCompany(name + " Company", taxId, address, address);
            return customer;
        }
        var contenders = new[] { Contender("First", "1234567890"), Contender("Second", "9876543210") };
        var before = await MongoStateAssertions.Snapshot(context);
        var outcomes = await Task.WhenAll(contenders.Select(customer => Observe(() => repository.CreateCustomer(customer, CancellationToken.None))));
        AssertOutcomes(outcomes);
        var winner = contenders[Array.FindIndex(outcomes, error => error is null)];
        await MongoStateAssertions.Winner(context, before, "customers", CustomerMapping.MapToDocument(winner).ToBsonDocument());
    }

    [Fact]
    public async Task ConcurrentAdminCreatesPreserveOnlyTheCompleteWinnerAndItsHistory()
    {
        await using var services = TestServiceProviderFactory.Create(fixture.ConnectionString, $"admin-create-race-{Guid.NewGuid():N}");
        await services.InitializeInfrastructureAsync();
        var repository = services.GetRequiredService<IAdminRepository>();
        var context = services.GetRequiredService<MongoDbContext>();
        var contenders = new[] { new Admin("shared-external-id", "First Admin", "first@example.com"), new Admin("shared-external-id", "Second Admin", "second@example.com") };
        var before = await MongoStateAssertions.Snapshot(context);
        var outcomes = await Task.WhenAll(contenders.Select(admin => Observe(() => repository.CreateAdmin(admin, CancellationToken.None))));
        AssertOutcomes(outcomes);
        var winner = contenders[Array.FindIndex(outcomes, error => error is null)];
        await MongoStateAssertions.Winner(context, before, "admins", AdminMapping.MapToDocument(winner).ToBsonDocument());
    }

    private static async Task<Exception?> Observe<T>(Func<Task<T>> write)
    {
        try { await write(); return null; }
        catch (Exception error) { return error; }
    }

    private void AssertOutcomes(Exception?[] outcomes)
    {
        outcomes.Count(error => error is null).ShouldBe(1);
        var conflict = outcomes.Where(error => error is not null).ShouldHaveSingleItem()!;
        output.WriteLine($"Losing create: {conflict.GetType().Name}.");
        if (conflict is ResourceAlreadyExistsException duplicate)
        {
            duplicate.ResourceId.ShouldBe("shared-external-id");
            duplicate.ResourceType.ShouldBeOneOf("Customer", "Admin");
            duplicate.ActionName.ShouldBe("Create" + duplicate.ResourceType);
        }
        else
            conflict.ShouldBeOfType<ConcurrencyConflictException>();
    }
}

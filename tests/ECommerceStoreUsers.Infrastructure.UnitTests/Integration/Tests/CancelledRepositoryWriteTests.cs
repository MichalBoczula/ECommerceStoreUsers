using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Infrastructure.Configuration;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Tests;

public sealed class CancelledRepositoryWriteTests(MongoDbTestFixture fixture) : IClassFixture<MongoDbTestFixture>
{
    [Theory]
    [InlineData("Customer")]
    [InlineData("Admin")]
    [InlineData("Favorite")]
    public async Task AlreadyCancelledDriverWritePreservesEveryCollection(string entity)
    {
        await using var services = TestServiceProviderFactory.Create(fixture.ConnectionString, $"cancel-write-{Guid.NewGuid():N}");
        await services.InitializeInfrastructureAsync();
        var customers = services.GetRequiredService<ICustomerRepository>();
        var admins = services.GetRequiredService<IAdminRepository>();
        var favorites = services.GetRequiredService<IFavoriteRepository>();
        var context = services.GetRequiredService<MongoDbContext>();
        var address = new Address("00-001", "Warsaw", "Retained Street", "10", null);
        var individual = new IndividualData("Original", "Person", "original@example.com", "123456789", address, address);
        await customers.CreateCustomer(new Customer("retained-customer", individual), CancellationToken.None);
        await admins.CreateAdmin(new Admin("retained-admin", "Original Admin", "original@example.com"), CancellationToken.None);
        await favorites.AddAsync(new Favorite(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
        var before = await MongoStateAssertions.Snapshot(context);
        before.Values.ShouldAllBe(documents => documents.Length == 1);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        async Task Write()
        {
            if (entity == "Customer")
                await customers.CreateCustomer(new Customer("cancelled-customer", individual), cancellation.Token);
            else if (entity == "Admin")
                await admins.CreateAdmin(new Admin("cancelled-admin", "Attempt Admin", "attempt@example.com"), cancellation.Token);
            else
                await favorites.AddAsync(new Favorite(Guid.NewGuid(), Guid.NewGuid()), cancellation.Token);
        }
        await Should.ThrowAsync<OperationCanceledException>(Write);
        await MongoStateAssertions.Unchanged(context, before);
    }
}

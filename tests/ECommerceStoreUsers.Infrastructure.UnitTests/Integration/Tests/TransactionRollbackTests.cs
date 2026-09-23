using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Persistence.Admins.History;
using ECommerceStoreUsers.Infrastructure.Persistence.Customers;
using ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Tests;

public sealed class TransactionRollbackTests(MongoDbTestFixture fixture) : IClassFixture<MongoDbTestFixture>
{
    [Fact]
    public async Task CustomerCreateHistoryFailureDoesNotLeaveCustomerBehind()
    {
        await using var services = TestServiceProviderFactory.Create(
            fixture.ConnectionString, $"customer-create-rollback-{Guid.NewGuid():N}");
        var repository = services.GetRequiredService<ICustomerRepository>();
        var context = services.GetRequiredService<MongoDbContext>();
        var address = new Address("00-001", "Warszawa", "Testowa", "10", "5");
        var customer = new Customer($"entra-id|{Guid.NewGuid()}",
            new IndividualData("Jan", "Kowalski", "jan@example.com", "+48123456789", address, address));

        await context.CustomersHistory.InsertOneAsync(
            CustomerMapping.MapToHistoryDocument(customer, ActionType.Insert));
        await context.CustomersHistory.Indexes.CreateOneAsync(
            new CreateIndexModel<CustomersHistoryDocument>(
                Builders<CustomersHistoryDocument>.IndexKeys.Ascending(x => x.CustomerId),
                new CreateIndexOptions { Unique = true }));

        var error = await Should.ThrowAsync<MongoWriteException>(() =>
            repository.CreateCustomer(customer, CancellationToken.None));

        error.WriteError.Code.ShouldBe(11000);
        (await repository.GetByIdAsync(customer.Id, CancellationToken.None)).ShouldBeNull();
        (await context.CustomersHistory.CountDocumentsAsync(x => x.CustomerId == customer.Id))
            .ShouldBe(1);
    }

    [Fact]
    public async Task CustomerHistoryFailureRollsBackUpdatedCustomerAndPreservesWriteError()
    {
        await using var services = TestServiceProviderFactory.Create(
            fixture.ConnectionString, $"customer-rollback-{Guid.NewGuid():N}");
        var repository = services.GetRequiredService<ICustomerRepository>();
        var context = services.GetRequiredService<MongoDbContext>();
        var address = new Address("00-001", "Warszawa", "Testowa", "10", "5");
        var customer = new Customer($"entra-id|{Guid.NewGuid()}",
            new IndividualData("Jan", "Kowalski", "jan@example.com", "+48123456789", address, address));
        await repository.CreateCustomer(customer, CancellationToken.None);

        await context.CustomersHistory.Indexes.CreateOneAsync(
            new CreateIndexModel<CustomersHistoryDocument>(
                Builders<CustomersHistoryDocument>.IndexKeys.Ascending(x => x.CustomerId),
                new CreateIndexOptions { Unique = true }));

        var updated = await repository.GetByIdAsync(customer.Id, CancellationToken.None);
        updated.ShouldNotBeNull();
        updated.UpdateIndividualData(
            new IndividualData("Anna", "Nowak", "anna@example.com", "+48123456789", address, address));

        var error = await Should.ThrowAsync<MongoWriteException>(() =>
            repository.UpdateCustomer(updated, CancellationToken.None));

        error.WriteError.Code.ShouldBe(11000);
        var persisted = await repository.GetByIdAsync(customer.Id, CancellationToken.None);
        persisted.ShouldNotBeNull();
        persisted.Individual.FirstName.ShouldBe("Jan");
        (await context.CustomersHistory.CountDocumentsAsync(x => x.CustomerId == customer.Id))
            .ShouldBe(1);
    }

    [Fact]
    public async Task AdminHistoryFailureRollsBackUpdatedAdminAndPreservesWriteError()
    {
        await using var services = TestServiceProviderFactory.Create(
            fixture.ConnectionString, $"admin-rollback-{Guid.NewGuid():N}");
        var repository = services.GetRequiredService<IAdminRepository>();
        var context = services.GetRequiredService<MongoDbContext>();
        var admin = new Admin($"entra-id|{Guid.NewGuid()}", "Test Admin", "admin@example.com");
        await repository.CreateAdmin(admin, CancellationToken.None);

        await context.AdminsHistory.Indexes.CreateOneAsync(
            new CreateIndexModel<AdminHistoryDocument>(
                Builders<AdminHistoryDocument>.IndexKeys.Ascending(x => x.AdminId),
                new CreateIndexOptions { Unique = true }));

        var updated = await repository.GetByIdAsync(admin.Id, CancellationToken.None);
        updated.ShouldNotBeNull();
        updated.Deactivate();

        var error = await Should.ThrowAsync<MongoWriteException>(() =>
            repository.UpdateAdmin(updated, CancellationToken.None));

        error.WriteError.Code.ShouldBe(11000);
        var persisted = await repository.GetByIdAsync(admin.Id, CancellationToken.None);
        persisted.ShouldNotBeNull();
        persisted.IsActive.ShouldBeTrue();
        (await context.AdminsHistory.CountDocumentsAsync(x => x.AdminId == admin.Id))
            .ShouldBe(1);
    }

    [Fact]
    public async Task AdminCreateHistoryFailureDoesNotLeaveAdminBehind()
    {
        await using var services = TestServiceProviderFactory.Create(
            fixture.ConnectionString, $"admin-create-rollback-{Guid.NewGuid():N}");
        var repository = services.GetRequiredService<IAdminRepository>();
        var context = services.GetRequiredService<MongoDbContext>();
        var admin = new Admin($"entra-id|{Guid.NewGuid()}", "Test Admin", "admin@example.com");

        await context.AdminsHistory.InsertOneAsync(
            AdminMapping.MapToHistoryDocument(admin, ActionType.Insert));
        await context.AdminsHistory.Indexes.CreateOneAsync(
            new CreateIndexModel<AdminHistoryDocument>(
                Builders<AdminHistoryDocument>.IndexKeys.Ascending(x => x.AdminId),
                new CreateIndexOptions { Unique = true }));

        var error = await Should.ThrowAsync<MongoWriteException>(() =>
            repository.CreateAdmin(admin, CancellationToken.None));

        error.WriteError.Code.ShouldBe(11000);
        (await repository.GetByIdAsync(admin.Id, CancellationToken.None)).ShouldBeNull();
        (await context.AdminsHistory.CountDocumentsAsync(x => x.AdminId == admin.Id))
            .ShouldBe(1);
    }
}

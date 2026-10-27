using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Infrastructure.Configuration;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Tests;

public sealed class CreateConflictTests(MongoDbTestFixture fixture) : IClassFixture<MongoDbTestFixture>
{
    [Theory]
    [InlineData("Customer", "ExternalId")]
    [InlineData("Admin", "ExternalId")]
    [InlineData("Customer", "Id")]
    [InlineData("Admin", "Id")]
    [InlineData("Customer", "IdAndExternalId")]
    [InlineData("Admin", "IdAndExternalId")]
    [InlineData("Customer", "OtherIndex")]
    [InlineData("Admin", "OtherIndex")]
    public async Task OnlyTheKnownExternalIdConstraintMapsToResourceConflict(string entity, string collision)
    {
        await using var services = TestServiceProviderFactory.Create(fixture.ConnectionString, $"create-conflict-{Guid.NewGuid():N}");
        var context = services.GetRequiredService<MongoDbContext>();
        var collection = entity == "Customer" ? "customers" : "admins";
        if (collision == "OtherIndex")
            await context.Customers.Database.GetCollection<BsonDocument>(collection).Indexes.CreateOneAsync(
                new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("ExternalId"),
                    new CreateIndexOptions { Unique = true, Name = "ref08_other_unique_index" }));
        else
            await services.InitializeInfrastructureAsync();

        var existingId = Guid.NewGuid();
        var duplicateId = collision is "Id" or "IdAndExternalId" ? existingId : Guid.NewGuid();
        var duplicateExternalId = collision == "Id" ? "different-external-id" : "existing-external-id";
        Func<Task> create;
        if (entity == "Customer")
        {
            var repository = services.GetRequiredService<ICustomerRepository>();
            var address = new Address("00-001", "Warsaw", "Original Street", "10", null);
            var individual = new IndividualData("Original", "Person", "original@example.com", "123456789", address, address);
            await repository.CreateCustomer(new Customer("existing-external-id", individual) { Id = existingId }, CancellationToken.None);
            var duplicate = new Customer(duplicateExternalId, individual) { Id = duplicateId };
            create = async () => { await repository.CreateCustomer(duplicate, CancellationToken.None); };
        }
        else
        {
            var repository = services.GetRequiredService<IAdminRepository>();
            await repository.CreateAdmin(new Admin("existing-external-id", "Original Admin", "original@example.com") { Id = existingId }, CancellationToken.None);
            var duplicate = new Admin(duplicateExternalId, "Attempt Admin", "attempt@example.com") { Id = duplicateId };
            create = async () => { await repository.CreateAdmin(duplicate, CancellationToken.None); };
        }
        var before = await MongoStateAssertions.Snapshot(context);
        var error = await Record.ExceptionAsync(create);
        error.ShouldNotBeNull();
        if (collision == "ExternalId")
        {
            var conflict = error.ShouldBeOfType<ResourceAlreadyExistsException>();
            conflict.ResourceType.ShouldBe(entity);
            conflict.ResourceId.ShouldBe(duplicateExternalId);
            conflict.ActionName.ShouldBe("Create" + entity);
        }
        else
        {
            var mongo = error.ShouldBeOfType<MongoWriteException>();
            mongo.WriteError.Code.ShouldBe(11000);
            mongo.WriteError.Message.ShouldContain(collision == "OtherIndex" ? "ref08_other_unique_index" : "index: _id_");
        }
        await MongoStateAssertions.Unchanged(context, before);
    }
}

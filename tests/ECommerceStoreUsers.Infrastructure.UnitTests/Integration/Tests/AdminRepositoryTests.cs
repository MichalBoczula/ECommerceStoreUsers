using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Tests
{
    public sealed class AdminRepositoryTests : IClassFixture<MongoDbTestFixture>
    {
        private readonly MongoDbTestFixture _fixture;

        public AdminRepositoryTests(MongoDbTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreateAdmin_ShouldSaveAdminToDatabase()
        {
            // arrange
            var databaseName = $"admin-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<IAdminRepository>();

            var admin = CreateTestAdmin();

            // act
            await repository.CreateAdmin(admin, CancellationToken.None);

            // assert
            var result = await repository.GetByIdAsync(admin.Id, CancellationToken.None);

            result.ShouldNotBeNull();
            result.Id.ShouldBe(admin.Id);
            result.ExternalId.ShouldBe(admin.ExternalId);
            result.FullName.ShouldBe(admin.FullName);
            result.Email.ShouldBe(admin.Email);
            result.IsActive.ShouldBeTrue();
        }

        [Fact]
        public async Task UpdateAdmin_ShouldModifyExistingDatabaseDocument()
        {
            // arrange
            var databaseName = $"admin-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<IAdminRepository>();

            var admin = CreateTestAdmin();
            await repository.CreateAdmin(admin, CancellationToken.None);

            var adminToUpdate = await repository.GetByIdAsync(admin.Id, CancellationToken.None);
            adminToUpdate.ShouldNotBeNull();

            adminToUpdate.Deactivate();

            // act
            await repository.UpdateAdmin(adminToUpdate, CancellationToken.None);

            // assert
            var updatedResult = await repository.GetByIdAsync(admin.Id, CancellationToken.None);
            updatedResult.ShouldNotBeNull();
            updatedResult.IsActive.ShouldBeFalse();
        }


        [Fact]
        public async Task GetByIdAsync_ShouldReturnAdmin_WhenMatchExists()
        {
            // arrange
            var databaseName = $"admin-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<IAdminRepository>();

            var admin = CreateTestAdmin();
            await repository.CreateAdmin(admin, CancellationToken.None);

            // act
            var result = await repository.GetByIdAsync(admin.Id, CancellationToken.None);

            // assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(admin.Id);
            result.ExternalId.ShouldBe(admin.ExternalId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenAdminDoesNotExist()
        {
            // arrange
            var databaseName = $"admin-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<IAdminRepository>();

            var nonExistentId = Guid.NewGuid();

            // act
            var result = await repository.GetByIdAsync(nonExistentId, CancellationToken.None);

            // assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetByExternalIdAsync_ShouldReturnAdmin_WhenMatchExists()
        {
            // arrange
            var databaseName = $"admin-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<IAdminRepository>();

            var targetExternalId = $"entra-id|{Guid.NewGuid()}";
            var admin = new Admin(targetExternalId, "Super Admin", "admin@store.com");
            await repository.CreateAdmin(admin, CancellationToken.None);

            // act
            var result = await repository.GetByExternalIdAsync(targetExternalId, CancellationToken.None);

            // assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(admin.Id);
            result.ExternalId.ShouldBe(targetExternalId);
        }

        [Fact]
        public async Task GetByExternalIdAsync_ShouldReturnNull_WhenExternalIdDoesNotExist()
        {
            // arrange
            var databaseName = $"admin-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<IAdminRepository>();

            var nonExistentExternalId = $"entra-id|non-existent-{Guid.NewGuid()}";

            // act
            var result = await repository.GetByExternalIdAsync(nonExistentExternalId, CancellationToken.None);

            // assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task UpdateAdmin_ShouldPersistDeactivationAndReactivationFlow()
        {
            // arrange
            var databaseName = $"admin-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<IAdminRepository>();

            var admin = CreateTestAdmin();
            await repository.CreateAdmin(admin, CancellationToken.None);

            // 1. Deactivate
            var loadedAdmin = await repository.GetByIdAsync(admin.Id, CancellationToken.None);
            loadedAdmin!.Deactivate();
            await repository.UpdateAdmin(loadedAdmin, CancellationToken.None);

            var deactivatedResult = await repository.GetByIdAsync(admin.Id, CancellationToken.None);
            deactivatedResult!.IsActive.ShouldBeFalse();

            // 2. Reactivate
            deactivatedResult.Activate();
            await repository.UpdateAdmin(deactivatedResult, CancellationToken.None);

            // assert
            var reactivatedResult = await repository.GetByIdAsync(admin.Id, CancellationToken.None);
            reactivatedResult!.IsActive.ShouldBeTrue();
        }

        [Fact]
        public async Task UpdateAdmin_ShouldPersistNewTimestamp_WhenLoginIsRecorded()
        {
            // arrange
            var databaseName = $"admin-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<IAdminRepository>();

            // Artificially rehydrate an admin with an older login date to verify a clear change date
            var historicalDate = DateTime.UtcNow.AddDays(-5);
            var admin = Admin.Rehydrate(Guid.NewGuid(), $"entra-id|{Guid.NewGuid()}", "Test Admin", "test@test.com", true, historicalDate);
            await repository.CreateAdmin(admin, CancellationToken.None);

            var adminToUpdate = await repository.GetByIdAsync(admin.Id, CancellationToken.None);
            adminToUpdate.ShouldNotBeNull();

            // act
            adminToUpdate.RecordLogin();
            await repository.UpdateAdmin(adminToUpdate, CancellationToken.None);

            // assert
            var result = await repository.GetByIdAsync(admin.Id, CancellationToken.None);
            result.ShouldNotBeNull();
            result.LastLoginAt.ShouldBeGreaterThan(historicalDate);
        }

        private static Admin CreateTestAdmin() =>
            new(
                externalId: $"entra-id|{Guid.NewGuid()}",
                fullName: "Piotr Nowak",
                email: "piotr.nowak@ecommerce.pl"
            );
    }
}
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Persistence.Admins;
using ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
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

        [Fact]
        public async Task CreateAdmin_ShouldCreateHistoryRecordWithInsertAction()
        {
            // arrange
            var databaseName = $"admin-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<IAdminRepository>();
            var context = serviceProvider.GetRequiredService<MongoDbContext>();

            var admin = CreateTestAdmin();

            // act
            await repository.CreateAdmin(admin, CancellationToken.None);

            // assert
            var historyRecord = await context.AdminsHistory
                .Find(x => x.AdminId == admin.Id)
                .FirstOrDefaultAsync(CancellationToken.None);

            historyRecord.ShouldNotBeNull();
            historyRecord.Id.ShouldNotBe(Guid.Empty);
            historyRecord.AdminId.ShouldBe(admin.Id);
            historyRecord.FullName.ShouldBe(admin.FullName);
            historyRecord.Email.ShouldBe(admin.Email);
            historyRecord.Action.ShouldBe(ActionType.Insert);
            historyRecord.ChangedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        }

        [Fact]
        public async Task UpdateAdmin_ShouldAppendNewHistoryRecordWithUpdateAction()
        {
            // arrange
            var databaseName = $"admin-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<IAdminRepository>();
            var context = serviceProvider.GetRequiredService<MongoDbContext>();

            var admin = CreateTestAdmin();
            await repository.CreateAdmin(admin, CancellationToken.None);

            var adminToUpdate = await repository.GetByIdAsync(admin.Id, CancellationToken.None);
            adminToUpdate!.Deactivate();

            // act
            await repository.UpdateAdmin(adminToUpdate, CancellationToken.None);

            // assert
            var historyRecords = await context.AdminsHistory
                .Find(x => x.AdminId == admin.Id)
                .SortBy(x => x.ChangedAt)
                .ToListAsync(CancellationToken.None);

            historyRecords.Count.ShouldBe(2);

            var initialRecord = historyRecords[0];
            initialRecord.Action.ShouldBe(ActionType.Insert);
            initialRecord.IsActive.ShouldBeTrue();

            var updatedRecord = historyRecords[1];
            updatedRecord.Action.ShouldBe(ActionType.Update);
            updatedRecord.IsActive.ShouldBeFalse();
            updatedRecord.Version.ShouldBe(1);
        }

        [Fact]
        public async Task UpdateAdmin_WithStaleVersion_ShouldPreserveFirstWriteAndHistory()
        {
            await using var services = TestServiceProviderFactory.Create(
                _fixture.ConnectionString, $"admin-tests-{Guid.NewGuid():N}");
            var repository = services.GetRequiredService<IAdminRepository>();
            var context = services.GetRequiredService<MongoDbContext>();
            var original = CreateTestAdmin();
            await repository.CreateAdmin(original, CancellationToken.None);
            var first = await repository.GetByIdAsync(original.Id, CancellationToken.None);
            var second = await repository.GetByIdAsync(original.Id, CancellationToken.None);
            first.ShouldNotBeNull();
            second.ShouldNotBeNull();
            first.Deactivate();
            second.RecordLogin();

            await repository.UpdateAdmin(first, CancellationToken.None);
            await Should.ThrowAsync<ConcurrencyConflictException>(() =>
                repository.UpdateAdmin(second, CancellationToken.None));

            first.Version.ShouldBe(1);
            second.Version.ShouldBe(0);
            var saved = await repository.GetByIdAsync(original.Id, CancellationToken.None);
            saved.ShouldNotBeNull();
            saved.IsActive.ShouldBeFalse();
            saved.Version.ShouldBe(1);
            (await context.AdminsHistory.CountDocumentsAsync(x => x.AdminId == original.Id)).ShouldBe(2);
        }

        [Fact]
        public async Task ConcurrentAdminUpdates_ShouldCommitOnlyOneVersionAndHistoryEvent()
        {
            await using var services = TestServiceProviderFactory.Create(
                _fixture.ConnectionString, $"admin-tests-{Guid.NewGuid():N}");
            var repository = services.GetRequiredService<IAdminRepository>();
            var context = services.GetRequiredService<MongoDbContext>();
            var original = CreateTestAdmin();
            await repository.CreateAdmin(original, CancellationToken.None);
            var first = await repository.GetByIdAsync(original.Id, CancellationToken.None);
            var second = await repository.GetByIdAsync(original.Id, CancellationToken.None);
            first.ShouldNotBeNull();
            second.ShouldNotBeNull();
            first.Deactivate();
            second.RecordLogin();

            async Task<Exception?> Attempt(Admin admin)
            {
                try { await repository.UpdateAdmin(admin, CancellationToken.None); return null; }
                catch (Exception exception) { return exception; }
            }

            var outcomes = await Task.WhenAll(Attempt(first), Attempt(second));
            outcomes.Count(x => x is null).ShouldBe(1);
            outcomes.Count(x => x is ConcurrencyConflictException).ShouldBe(1);
            (await repository.GetByIdAsync(original.Id, CancellationToken.None))!.Version.ShouldBe(1);
            (await context.AdminsHistory.CountDocumentsAsync(x => x.AdminId == original.Id)).ShouldBe(2);
        }

        [Fact]
        public async Task UpdateAdmin_WhenDeletedAfterRead_ShouldReturnNotFoundWithoutHistory()
        {
            await using var services = TestServiceProviderFactory.Create(
                _fixture.ConnectionString, $"admin-tests-{Guid.NewGuid():N}");
            var repository = services.GetRequiredService<IAdminRepository>();
            var context = services.GetRequiredService<MongoDbContext>();
            var original = CreateTestAdmin();
            await repository.CreateAdmin(original, CancellationToken.None);
            var loaded = await repository.GetByIdAsync(original.Id, CancellationToken.None);
            loaded.ShouldNotBeNull();
            loaded.Deactivate();
            await context.Admins.DeleteOneAsync(x => x.Id == original.Id);

            await Should.ThrowAsync<ResourceNotFoundException>(() =>
                repository.UpdateAdmin(loaded, CancellationToken.None));
            (await context.AdminsHistory.CountDocumentsAsync(x => x.AdminId == original.Id)).ShouldBe(1);
        }

        [Fact]
        public async Task UpdateAdmin_UnchangedProfile_ShouldNotAdvanceVersionOrHistory()
        {
            await using var services = TestServiceProviderFactory.Create(
                _fixture.ConnectionString, $"admin-tests-{Guid.NewGuid():N}");
            var repository = services.GetRequiredService<IAdminRepository>();
            var context = services.GetRequiredService<MongoDbContext>();
            var original = CreateTestAdmin();
            await repository.CreateAdmin(original, CancellationToken.None);
            var loaded = await repository.GetByIdAsync(original.Id, CancellationToken.None);
            loaded.ShouldNotBeNull();

            await repository.UpdateAdmin(loaded, CancellationToken.None);
            await repository.UpdateAdmin(loaded, CancellationToken.None);

            loaded.Version.ShouldBe(0);
            (await repository.GetByIdAsync(original.Id, CancellationToken.None))!.Version.ShouldBe(0);
            (await context.AdminsHistory.CountDocumentsAsync(x => x.AdminId == original.Id)).ShouldBe(1);
        }

        [Fact]
        public async Task UpdateAdmin_LegacyDocumentWithoutVersion_ShouldUpgradeOnWrite()
        {
            await using var services = TestServiceProviderFactory.Create(
                _fixture.ConnectionString, $"admin-tests-{Guid.NewGuid():N}");
            var repository = services.GetRequiredService<IAdminRepository>();
            var context = services.GetRequiredService<MongoDbContext>();
            var original = CreateTestAdmin();
            await repository.CreateAdmin(original, CancellationToken.None);
            await context.Admins.UpdateOneAsync(x => x.Id == original.Id,
                Builders<AdminDocument>.Update.Unset(x => x.Version));
            var loaded = await repository.GetByIdAsync(original.Id, CancellationToken.None);
            loaded.ShouldNotBeNull();
            loaded.Version.ShouldBe(0);
            loaded.Deactivate();

            await repository.UpdateAdmin(loaded, CancellationToken.None);

            (await repository.GetByIdAsync(original.Id, CancellationToken.None))!.Version.ShouldBe(1);
            (await context.AdminsHistory.CountDocumentsAsync(x => x.AdminId == original.Id)).ShouldBe(2);
        }

        private static Admin CreateTestAdmin() =>
            new(
                externalId: $"entra-id|{Guid.NewGuid()}",
                fullName: "Piotr Nowak",
                email: "piotr.nowak@ecommerce.pl"
            );
    }
}

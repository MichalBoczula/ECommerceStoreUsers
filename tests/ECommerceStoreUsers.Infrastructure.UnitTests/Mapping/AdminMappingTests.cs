using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Persistance.Admins;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Mapping;

public sealed class AdminMappingTests
{
    [Fact]
    public void MapToDocument_ShouldMapAllAdminFields()
    {
        var admin = CreateAdmin();

        var document = AdminMapping.MapToDocument(admin);

        document.Id.ShouldBe(admin.Id);
        document.ExternalId.ShouldBe(admin.ExternalId);
        document.FullName.ShouldBe(admin.FullName);
        document.Email.ShouldBe(admin.Email);
        document.IsActive.ShouldBe(admin.IsActive);
        document.LastLoginAt.ShouldBe(admin.LastLoginAt);
    }

    [Fact]
    public void MapToDomain_ShouldMapAllAdminFields()
    {
        var document = CreateAdminDocument();

        var admin = AdminMapping.MapToDomain(document);

        admin.Id.ShouldBe(document.Id);
        admin.ExternalId.ShouldBe(document.ExternalId);
        admin.FullName.ShouldBe(document.FullName);
        admin.Email.ShouldBe(document.Email);
        admin.IsActive.ShouldBe(document.IsActive);
        admin.LastLoginAt.ShouldBe(document.LastLoginAt);
    }

    [Fact]
    public void MapToHistoryDocument_ShouldMapAllAdminFieldsAndHistoryMetadata()
    {
        var admin = CreateAdmin();
        var beforeMapping = DateTime.UtcNow;

        var document = AdminMapping.MapToHistoryDocument(admin, ActionType.Update);
        var afterMapping = DateTime.UtcNow;

        document.Id.ShouldNotBe(Guid.Empty);
        document.AdminId.ShouldBe(admin.Id);
        document.ExternalId.ShouldBe(admin.ExternalId);
        document.FullName.ShouldBe(admin.FullName);
        document.Email.ShouldBe(admin.Email);
        document.IsActive.ShouldBe(admin.IsActive);
        document.LastLoginAt.ShouldBe(admin.LastLoginAt);
        document.ChangedAt.ShouldBeInRange(beforeMapping, afterMapping);
        document.Action.ShouldBe(ActionType.Update);
    }

    private static Admin CreateAdmin() => Admin.Rehydrate(
        Guid.NewGuid(),
        "external-admin-1",
        "Anna Nowak",
        "anna.nowak@example.com",
        false,
        new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc));

    private static AdminDocument CreateAdminDocument() => new()
    {
        Id = Guid.NewGuid(),
        ExternalId = "external-admin-2",
        FullName = "Jan Kowalski",
        Email = "jan.kowalski@example.com",
        IsActive = true,
        LastLoginAt = new DateTime(2026, 4, 20, 10, 30, 0, DateTimeKind.Utc)
    };
}

using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Infrastructure.Persistence.Admins;
using ECommerceStoreUsers.Infrastructure.Persistence.Admins.History;

namespace ECommerceStoreUsers.Infrastructure.Mapping
{
    internal static class AdminMapping
    {
        internal static AdminDocument MapToDocument(Admin admin)
        {
            return new AdminDocument
            {
                Id = admin.Id,
                ExternalId = admin.ExternalId,
                FullName = admin.FullName,
                Email = admin.Email,
                IsActive = admin.IsActive,
                LastLoginAt = admin.LastLoginAt,
                Version = admin.Version
            };
        }

        internal static Admin MapToDomain(AdminDocument adminDocument)
        {
            return Admin.Rehydrate(
                adminDocument.Id,
                adminDocument.ExternalId,
                adminDocument.FullName,
                adminDocument.Email,
                adminDocument.IsActive,
                adminDocument.LastLoginAt,
                adminDocument.Version
            );
        }

        internal static AdminHistoryDocument MapToHistoryDocument(Admin admin, ActionType action, long? version = null)
        {
            return new AdminHistoryDocument
            {
                Id = Guid.NewGuid(),
                AdminId = admin.Id,
                ExternalId = admin.ExternalId,
                FullName = admin.FullName,
                Email = admin.Email,
                IsActive = admin.IsActive,
                LastLoginAt = admin.LastLoginAt,
                Version = version ?? admin.Version,
                ChangedAt = DateTime.UtcNow,
                Action = action
            };
        }
    }
}

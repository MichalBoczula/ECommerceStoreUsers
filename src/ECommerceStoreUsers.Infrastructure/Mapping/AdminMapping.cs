using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Infrastructure.Persistance.Admins;
using ECommerceStoreUsers.Infrastructure.Persistance.Admins.History;

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
                LastLoginAt = admin.LastLoginAt
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
                adminDocument.LastLoginAt
            );
        }

        internal static AdminHistoryDocument MapToHistoryDocument(Admin admin, ActionType action)
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
                ChangedAt = DateTime.UtcNow,
                Action = action
            };
        }
    }
}

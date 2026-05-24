using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Infrastructure.Persistance.Admins;

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
    }
}

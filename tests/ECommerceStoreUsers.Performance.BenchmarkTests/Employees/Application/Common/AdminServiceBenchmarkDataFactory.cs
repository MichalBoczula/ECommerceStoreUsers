using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Application.Common
{
    internal static class AdminServiceBenchmarkDataFactory
    {
        public static CreateAdminRequestDto CreateRequest()
        {
            return new CreateAdminRequestDto
            {
                ExternalId = "entra-id|admin-999",
                FullName = "Super Admin",
                Email = "super.admin@store.com"
            };
        }

        public static UpdateAdminProfileRequestDto UpdateRequest()
        {
            return new UpdateAdminProfileRequestDto
            {
                FullName = "Updated Admin Name",
                Email = "updated.admin@store.com"
            };
        }

        public static Admin CreateDomainAdmin(Guid id, string externalId)
        {
            return Admin.Rehydrate(
                id,
                externalId,
                "Super Admin",
                "super.admin@store.com",
                true,
                DateTime.UtcNow.AddHours(-1));
        }
    }
}
using ECommerceStoreUsers.Infrastructure.Persistance.Admins;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Infrastructure.Common
{
    internal static class AdminDocumentBenchmarkDataFactory
    {
        private static readonly DateTime BenchmarkDate = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        public static AdminDocument Create(Guid id, string externalId)
        {
            return new AdminDocument
            {
                Id = id,
                ExternalId = externalId,
                FullName = "Jan Kowalski",
                Email = "jan.kowalski@store.com",
                IsActive = true,
                LastLoginAt = BenchmarkDate
            };
        }

        public static AdminDocument Create() => Create(Guid.NewGuid(), $"entra-id|{Guid.NewGuid()}");
    }
}

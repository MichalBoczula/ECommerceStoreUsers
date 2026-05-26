using ECommerceStoreUsers.Domain.AggregatesModel.Employees;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Domain
{
    internal static class AdminValidationDataFactory
    {
        public static Admin CreateValid()
        {
            return new Admin(
                externalId: "entra-id|9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
                fullName: "Jan Kowalski",
                email: "jan.kowalski@store.com");
        }

        public static Admin CreateInvalidEmail()
        {
            return new Admin(
                externalId: "entra-id|9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
                fullName: "Jan Kowalski",
                email: "invalid-email-format");
        }

        public static Admin CreateAllInvalid()
        {
            return Admin.Rehydrate(
                id: Guid.NewGuid(),
                externalId: "   ",
                fullName: "Jan@Kowalski#",
                email: "bad@@email.com",
                isActive: true,
                lastLoginAt: DateTime.UtcNow);
        }
    }
}

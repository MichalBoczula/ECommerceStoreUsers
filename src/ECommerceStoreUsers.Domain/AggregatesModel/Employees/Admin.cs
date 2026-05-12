namespace ECommerceStoreUsers.Domain.AggregatesModel.Employees
{
    public sealed class Admin
    {
        public Guid Id { get; init; }
        public string ExternalId { get; init; }
        public string FullName { get; init; }
        public string Email { get; init; }
        public bool IsActive { get; private set; }
        public DateTime LastLoginAt { get; private set; }

        public Admin(string externalId, string fullName, string email)
        {
            Id = Guid.NewGuid();
            ExternalId = externalId;
            FullName = fullName;
            Email = email;
            IsActive = true;
            LastLoginAt = DateTime.UtcNow;
        }

        private Admin(
            Guid id,
            string externalId,
            string fullName,
            string email,
            bool isActive,
            DateTime lastLoginAt)
        {
            Id = id;
            ExternalId = externalId;
            FullName = fullName;
            Email = email;
            IsActive = isActive;
            LastLoginAt = lastLoginAt;
        }

        public void RecordLogin()
        {
            if (!IsActive) throw new InvalidOperationException("Cannot record login for inactive admin.");
            LastLoginAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public static Admin Rehydrate(
            Guid id,
            string externalId,
            string fullName,
            string email,
            bool isActive,
            DateTime lastLoginAt)
        {
            return new Admin(
                id,
                externalId,
                fullName,
                email,
                isActive,
                lastLoginAt);
        }
    }
}

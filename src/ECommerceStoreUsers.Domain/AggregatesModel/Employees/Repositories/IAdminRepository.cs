namespace ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories
{
    public interface IAdminRepository
    {
        Task<Admin?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);
        Task<Admin> CreateAdmin(Admin admin, CancellationToken cancellationToken);
        Task<Admin> UpdateAdmin(Admin admin, CancellationToken cancellationToken);
    }
}

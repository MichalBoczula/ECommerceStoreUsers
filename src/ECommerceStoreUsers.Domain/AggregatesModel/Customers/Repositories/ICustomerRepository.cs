namespace ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Customer?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);
        Task<Customer> CreateCustomer(Customer customer, CancellationToken cancellationToken);
        Task<Customer> UpdateCustomer(Customer customer, CancellationToken cancellationToken);
    }
}

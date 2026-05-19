using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using MongoDB.Driver;

namespace ECommerceStoreUsers.Infrastructure.Repositories;

internal sealed class CustomerRepository : ICustomerRepository
{
    private readonly MongoDbContext _context;

    public CustomerRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customerDocument = await _context.Customers
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (customerDocument is null)
            return null;

        return CustomerMapping.MapToDomain(customerDocument);
    }

    public async Task<Customer?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        var customerDocument = await _context.Customers
            .Find(x => x.ExternalId == externalId)
            .FirstOrDefaultAsync();

        if (customerDocument is null)
            return null;

        return CustomerMapping.MapToDomain(customerDocument);
    }

    public async Task<Customer> CreateCustomer(Customer customer, CancellationToken cancellationToken)
    {
        var customerDocument = CustomerMapping.MapToDocument(customer);

        await _context.Customers.InsertOneAsync(customerDocument);

        return customer;
    }

    public async Task<Customer> UpdateCustomer(Customer customer, CancellationToken cancellationToken)
    {
        var customerDocument = CustomerMapping.MapToDocument(customer);

        var result = await _context.Customers.ReplaceOneAsync(
            x => x.Id == customerDocument.Id,
            customerDocument);

        if (result.MatchedCount == 0)
            throw new InvalidOperationException($"Customer with id '{customer.Id}' was not found.");

        return customer;
    }
}
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using MongoDB.Driver;

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
            .FirstOrDefaultAsync(cancellationToken);

        return customerDocument is null ? null : CustomerMapping.MapToDomain(customerDocument);
    }

    public async Task<Customer?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        var customerDocument = await _context.Customers
            .Find(x => x.ExternalId == externalId)
            .FirstOrDefaultAsync(cancellationToken);

        return customerDocument is null ? null : CustomerMapping.MapToDomain(customerDocument);
    }

    public async Task<Customer> CreateCustomer(Customer customer, CancellationToken cancellationToken)
    {
        var customerDocument = CustomerMapping.MapToDocument(customer);
        var historyDocument = CustomerMapping.MapToHistoryDocument(customer, ActionType.Insert);

        using var session = await _context.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();

        try
        {
            await _context.Customers.InsertOneAsync(session, customerDocument, cancellationToken: cancellationToken);
            await _context.CustomersHistory.InsertOneAsync(session, historyDocument, cancellationToken: cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);
            return customer;
        }
        catch
        {
            await session.AbortTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Customer> UpdateCustomer(Customer customer, CancellationToken cancellationToken)
    {
        var customerDocument = CustomerMapping.MapToDocument(customer);
        var historyDocument = CustomerMapping.MapToHistoryDocument(customer, ActionType.Update);

        using var session = await _context.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();

        try
        {
            await _context.Customers.ReplaceOneAsync(
                session,
                x => x.Id == customerDocument.Id,
                customerDocument,
                cancellationToken: cancellationToken);

            await _context.CustomersHistory.InsertOneAsync(session, historyDocument, cancellationToken: cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);
            return customer;
        }
        catch
        {
            await session.AbortTransactionAsync(cancellationToken);
            throw;
        }
    }
}
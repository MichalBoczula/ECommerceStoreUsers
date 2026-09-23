using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Persistence.Customers;
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
            await MongoTransactionAbort.TryAbortAsync(session.AbortTransactionAsync);
            throw;
        }
    }

    public async Task<Customer> UpdateCustomer(Customer customer, CancellationToken cancellationToken)
    {
        var customerDocument = CustomerMapping.MapToDocument(customer) with { Version = checked(customer.Version + 1) };
        var historyDocument = CustomerMapping.MapToHistoryDocument(customer, ActionType.Update, customerDocument.Version);
        var filter = Builders<CustomerDocument>.Filter;
        var versionFilter = filter.Eq(x => x.Version, customer.Version);
        // Older documents have no Version field; MongoDB deserializes them as version zero.
        if (customer.Version == 0)
            versionFilter = filter.Or(versionFilter, filter.Exists(x => x.Version, false));

        using var session = await _context.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();

        try
        {
            var result = await _context.Customers.ReplaceOneAsync(
                session,
                filter.And(filter.Eq(x => x.Id, customerDocument.Id), versionFilter),
                customerDocument,
                cancellationToken: cancellationToken);

            if (result.MatchedCount == 0)
            {
                var exists = await _context.Customers.Find(session, x => x.Id == customer.Id)
                    .AnyAsync(cancellationToken);
                if (!exists)
                    throw new ResourceNotFoundException(nameof(UpdateCustomer), customer.Id.ToString(), nameof(Customer));

                throw new ConcurrencyConflictException();
            }

            await _context.CustomersHistory.InsertOneAsync(session, historyDocument, cancellationToken: cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);
            customer.MarkPersisted();
            return customer;
        }
        catch (MongoCommandException exception) when (exception.Code == 112)
        {
            await MongoTransactionAbort.TryAbortAsync(session.AbortTransactionAsync);
            throw new ConcurrencyConflictException();
        }
        catch (MongoWriteException exception) when (exception.WriteError.Code == 112)
        {
            await MongoTransactionAbort.TryAbortAsync(session.AbortTransactionAsync);
            throw new ConcurrencyConflictException();
        }
        catch
        {
            await MongoTransactionAbort.TryAbortAsync(session.AbortTransactionAsync);
            throw;
        }
    }
}

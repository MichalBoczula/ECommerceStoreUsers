using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers;
using MongoDB.Driver;

namespace ECommerceStoreUsers.Infrastructure.Configuration;

internal sealed class MongoInitializer
{
    private readonly MongoDbContext _context;

    public MongoInitializer(MongoDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await CreateCustomerIndexesAsync(cancellationToken);
    }

    private async Task CreateCustomerIndexesAsync(CancellationToken cancellationToken)
    {
        var externalIdIndex = new CreateIndexModel<CustomerDocument>(
            Builders<CustomerDocument>.IndexKeys.Ascending(x => x.ExternalId),
            new CreateIndexOptions
            {
                Unique = true,
                Name = "UX_Customer_ExternalId"
            });

        var companyTaxIdIndex = new CreateIndexModel<CustomerDocument>(
            Builders<CustomerDocument>.IndexKeys.Ascending("Companies.TaxId"),
            new CreateIndexOptions
            {
                Name = "IX_Customer_Companies_TaxId"
            });

        await _context.Customers.Indexes.CreateManyAsync(
            new[] { externalIdIndex, companyTaxIdIndex },
            cancellationToken: cancellationToken);
    }
}
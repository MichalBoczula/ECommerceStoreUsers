using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Persistance.Admins;
using ECommerceStoreUsers.Infrastructure.Persistance.Admins.History;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers;
using MongoDB.Driver;

namespace ECommerceStoreUsers.Infrastructure.Configuration
{
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
            await CreateCustomersHistoryIndexesAsync(cancellationToken);
            await CreateAdminIndexesAsync(cancellationToken);
            await CreateAdminHistoryIndexesAsync(cancellationToken);
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

        private async Task CreateCustomersHistoryIndexesAsync(CancellationToken cancellationToken)
        {
            var customerHistoryCustomerIdIndex = new CreateIndexModel<CustomersHistoryDocument>(
                Builders<CustomersHistoryDocument>.IndexKeys.Ascending(x => x.CustomerId),
                new CreateIndexOptions
                {
                    Name = "IX_CustomersHistory_CustomerId"
                });

            await _context.CustomersHistory.Indexes.CreateOneAsync(
                customerHistoryCustomerIdIndex,
                cancellationToken: cancellationToken);
        }

        private async Task CreateAdminIndexesAsync(CancellationToken cancellationToken)
        {
            var adminExternalIdIndex = new CreateIndexModel<AdminDocument>(
                Builders<AdminDocument>.IndexKeys.Ascending(x => x.ExternalId),
                new CreateIndexOptions
                {
                    Unique = true,
                    Name = "UX_Admin_ExternalId"
                });

            await _context.Admins.Indexes.CreateOneAsync(
                adminExternalIdIndex,
                cancellationToken: cancellationToken);
        }

        private async Task CreateAdminHistoryIndexesAsync(CancellationToken cancellationToken)
        {
            var adminHistoryIdIndex = new CreateIndexModel<AdminHistoryDocument>(
                Builders<AdminHistoryDocument>.IndexKeys.Ascending(x => x.AdminId),
                new CreateIndexOptions
                {
                    Name = "IX_AdminHistory_AdminId"
                });

            await _context.AdminsHistory.Indexes.CreateOneAsync(
                adminHistoryIdIndex,
                cancellationToken: cancellationToken);
        }
    }
}
using ECommerceStoreUsers.Infrastructure.Configuration;
using ECommerceStoreUsers.Infrastructure.Persistance.Admins;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ECommerceStoreUsers.Infrastructure.Context
{
    internal sealed class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDbSettings _settings;

        public MongoDbContext(IOptions<MongoDbSettings> options)
        {
            _settings = options.Value;

            if (string.IsNullOrWhiteSpace(_settings.ConnectionString))
                throw new InvalidOperationException("MongoDbSettings.ConnectionString is not configured.");

            if (string.IsNullOrWhiteSpace(_settings.DatabaseName))
                throw new InvalidOperationException("MongoDbSettings.DatabaseName is not configured.");

            if (string.IsNullOrWhiteSpace(_settings.CustomerCollectionName))
                throw new InvalidOperationException("MongoDbSettings.CustomerCollectionName is not configured.");

            if (string.IsNullOrWhiteSpace(_settings.CustomerHistoryCollectionName))
                throw new InvalidOperationException("MongoDbSettings.CustomerHistoryCollectionName   is not configured.");

            var client = new MongoClient(_settings.ConnectionString);
            _database = client.GetDatabase(_settings.DatabaseName);
        }

        public IMongoCollection<CustomerDocument> Customers =>
            _database.GetCollection<CustomerDocument>(_settings.CustomerCollectionName);

        public IMongoCollection<AdminDocument> Admins =>
           _database.GetCollection<AdminDocument>(_settings.AdminCollectionName);
    }
}

using ECommerceStoreUsers.Infrastructure.Configuration;
using ECommerceStoreUsers.Infrastructure.Persistance.Admins;
using ECommerceStoreUsers.Infrastructure.Persistance.Admins.History;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ECommerceStoreUsers.Infrastructure.Context
{
    internal sealed class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDbSettings _settings;
        public IMongoClient Client { get; }

        public MongoDbContext(IOptions<MongoDbSettings> options)
        {
            _settings = options.Value;

            if (string.IsNullOrWhiteSpace(_settings.ConnectionString))
                throw new InvalidOperationException("MongoDbSettings.ConnectionString is not configured.");

            if (string.IsNullOrWhiteSpace(_settings.DatabaseName))
                throw new InvalidOperationException("MongoDbSettings.DatabaseName is not configured.");

            if (string.IsNullOrWhiteSpace(_settings.CustomerCollectionName))
                throw new InvalidOperationException("MongoDbSettings.CustomerCollectionName is not configured.");

            if (string.IsNullOrWhiteSpace(_settings.CustomersHistoryCollectionName))
                throw new InvalidOperationException("MongoDbSettings.CustomersHistoryCollectionName is not configured.");

            if (string.IsNullOrWhiteSpace(_settings.AdminCollectionName))
                throw new InvalidOperationException("MongoDbSettings.AdminCollectionName is not configured.");
            
            if (string.IsNullOrWhiteSpace(_settings.AdminsHistoryCollectionName))
                throw new InvalidOperationException("MongoDbSettings.AdminsHistoryCollectionName is not configured.");

            Client = new MongoClient(_settings.ConnectionString);
            _database = Client.GetDatabase(_settings.DatabaseName);
        }

        public IMongoCollection<CustomerDocument> Customers =>
            _database.GetCollection<CustomerDocument>(_settings.CustomerCollectionName);

        public IMongoCollection<CustomersHistoryDocument> CustomersHistory =>
           _database.GetCollection<CustomersHistoryDocument>(_settings.CustomersHistoryCollectionName);

        public IMongoCollection<AdminDocument> Admins =>
           _database.GetCollection<AdminDocument>(_settings.AdminCollectionName);

        public IMongoCollection<AdminHistoryDocument> AdminsHistory =>
           _database.GetCollection<AdminHistoryDocument>(_settings.AdminsHistoryCollectionName);
    }
}

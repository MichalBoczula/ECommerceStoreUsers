using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Infrastructure.Configuration;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers;
using ECommerceStoreUsers.Infrastructure.Repositories;
using ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Infrastructure.Common;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Infrastructure
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class CustomerRepositoryBenchmarks
    {
        private const string DatabaseName = "customer-repository-benchmarks";
        private const string CustomerCollectionName = "customers";

        private MongoDbContainer _mongoContainer = null!;
        private MongoDbContext _context = null!;
        private CustomerRepository _repository = null!;

        private readonly List<Guid> _existingIds = new();
        private readonly List<string> _existingExternalIds = new();

        private int _readIdCounter;
        private int _readExternalIdCounter;
        private int _updateCounter;

        [GlobalSetup]
        public async Task Setup()
        {
            _mongoContainer = new MongoDbBuilder("mongo:8")
                .WithUsername("admin")
                .WithPassword("admin123")
                .WithCreateParameterModifier(p => p.HostConfig.Tmpfs = new Dictionary<string, string> { { "/data/db", "rw" } })
                .Build();

            await _mongoContainer.StartAsync();

            var settings = new MongoDbSettings
            {
                ConnectionString = _mongoContainer.GetConnectionString(),
                DatabaseName = DatabaseName,
                AdminCollectionName = "admins",
                CustomerCollectionName = CustomerCollectionName,
                CustomerHistoryCollectionName = "customer-history"
            };

            _context = new MongoDbContext(Options.Create(settings));
            _repository = new CustomerRepository(_context);

            var indexKeys = Builders<CustomerDocument>.IndexKeys.Ascending(x => x.ExternalId);
            await _context.Customers.Indexes.CreateOneAsync(new CreateIndexModel<CustomerDocument>(indexKeys, new CreateIndexOptions { Unique = true }));

            await SeedDataAsync();
        }

        private async Task SeedDataAsync()
        {
            await _context.Customers.DeleteManyAsync(Builders<CustomerDocument>.Filter.Empty);
            _existingIds.Clear();
            _existingExternalIds.Clear();
            _readIdCounter = 0;
            _readExternalIdCounter = 0;
            _updateCounter = 0;

            for (var i = 0; i < 200; i++)
            {
                var id = Guid.NewGuid();
                var externalId = $"entra-id|{Guid.NewGuid()}";
                var doc = CustomerDocumentBenchmarkDataFactory.Create(id, externalId);

                await _context.Customers.InsertOneAsync(doc);
                _existingIds.Add(id);
                _existingExternalIds.Add(externalId);
            }
        }

        [Benchmark]
        public async Task<Customer?> GetByIdAsync()
        {
            var id = _existingIds[_readIdCounter % _existingIds.Count];
            _readIdCounter++;
            return await _repository.GetByIdAsync(id, CancellationToken.None);
        }

        [Benchmark]
        public async Task<Customer?> GetByExternalIdAsync()
        {
            var externalId = _existingExternalIds[_readExternalIdCounter % _existingExternalIds.Count];
            _readExternalIdCounter++;
            return await _repository.GetByExternalIdAsync(externalId, CancellationToken.None);
        }

        [Benchmark]
        public async Task CreateCustomer()
        {
            var doc = CustomerDocumentBenchmarkDataFactory.Create();
            await _repository.CreateCustomer(CustomerMapping.MapToDomain(doc), CancellationToken.None);
        }

        [Benchmark]
        public async Task UpdateCustomer()
        {
            var id = _existingIds[_updateCounter % _existingIds.Count];
            _updateCounter++;

            var existing = await _repository.GetByIdAsync(id, CancellationToken.None);
            if (existing is null)
            {
                return;
            }

            existing.UpdateIndividualData(new IndividualData(
                "Updated",
                "Customer",
                $"updated.customer+{_updateCounter}@store.com",
                "+48123456789",
                new Address("00-100", "Warsaw", "Updated", "1", "10"),
                new Address("00-101", "Warsaw", "Updated", "2", "11")));

            await _repository.UpdateCustomer(existing, CancellationToken.None);
        }

        [GlobalCleanup]
        public async Task Cleanup() => await _mongoContainer.DisposeAsync();
    }
}

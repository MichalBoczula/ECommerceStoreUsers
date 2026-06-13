using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Infrastructure.Configuration;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Persistance.Admins;
using ECommerceStoreUsers.Infrastructure.Repositories;
using ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Infrastructure.Common;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Infrastructure
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class AdminRepositoryBenchmarks
    {
        private const string DatabaseName = "admin-repository-benchmarks";
        private const string AdminCollectionName = "admins";

        private MongoDbContainer _mongoContainer = null!;
        private MongoDbContext _context = null!;
        private AdminRepository _repository = null!;

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
                .WithReplicaSet()
                .WithCreateParameterModifier(p => p.HostConfig.Tmpfs = new Dictionary<string, string> { { "/data/db", "rw" } })
                .Build();

            await _mongoContainer.StartAsync();

            var settings = new MongoDbSettings
            {
                ConnectionString = _mongoContainer.GetConnectionString(),
                DatabaseName = DatabaseName,
                AdminCollectionName = AdminCollectionName,
                CustomerCollectionName = "customers",
                CustomersHistoryCollectionName = "customers-history",
                AdminsHistoryCollectionName = "admin-history"
            };

            _context = new MongoDbContext(Options.Create(settings));
            _repository = new AdminRepository(_context);

            var indexKeys = Builders<AdminDocument>.IndexKeys.Ascending(x => x.ExternalId);
            await _context.Admins.Indexes.CreateOneAsync(new CreateIndexModel<AdminDocument>(indexKeys, new CreateIndexOptions { Unique = true }));

            await SeedDataAsync();
        }

        private async Task SeedDataAsync()
        {
            await _context.Admins.DeleteManyAsync(Builders<AdminDocument>.Filter.Empty);
            _existingIds.Clear();
            _existingExternalIds.Clear();
            _readIdCounter = 0;
            _readExternalIdCounter = 0;
            _updateCounter = 0;

            for (var i = 0; i < 200; i++)
            {
                var id = Guid.NewGuid();
                var externalId = $"entra-id|{Guid.NewGuid()}";
                var doc = AdminDocumentBenchmarkDataFactory.Create(id, externalId);

                await _context.Admins.InsertOneAsync(doc);
                _existingIds.Add(id);
                _existingExternalIds.Add(externalId);
            }
        }

        [Benchmark]
        public async Task<Admin?> GetByIdAsync()
        {
            var id = _existingIds[_readIdCounter % _existingIds.Count];
            _readIdCounter++;
            return await _repository.GetByIdAsync(id, CancellationToken.None);
        }

        [Benchmark]
        public async Task<Admin?> GetByExternalIdAsync()
        {
            var externalId = _existingExternalIds[_readExternalIdCounter % _existingExternalIds.Count];
            _readExternalIdCounter++;
            return await _repository.GetByExternalIdAsync(externalId, CancellationToken.None);
        }

        [Benchmark]
        public async Task CreateAdmin()
        {
            var doc = AdminDocumentBenchmarkDataFactory.Create();
            await _repository.CreateAdmin(AdminMapping.MapToDomain(doc), CancellationToken.None);
        }

        [Benchmark]
        public async Task UpdateAdmin()
        {
            var id = _existingIds[_updateCounter % _existingIds.Count];
            _updateCounter++;

            var externalId = $"entra-id|{Guid.NewGuid()}";
            var admin = Admin.Rehydrate(id, externalId, "Updated Admin Name", "updated.admin@store.com", true, DateTime.UtcNow);

            await _repository.UpdateAdmin(admin, CancellationToken.None);
        }

        [GlobalCleanup]
        public async Task Cleanup() => await _mongoContainer.DisposeAsync();
    }
}
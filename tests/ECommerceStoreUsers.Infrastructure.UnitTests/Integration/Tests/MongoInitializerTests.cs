using ECommerceStoreUsers.Infrastructure.Configuration;
using ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Tests
{
    public sealed class MongoInitializerTests : IClassFixture<MongoDbTestFixture>
    {
        private readonly MongoDbTestFixture _fixture;

        public MongoInitializerTests(MongoDbTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task InitializeInfrastructureAsync_ShouldCreateExpectedIndexes()
        {
            var databaseName = $"user-tests-{Guid.NewGuid():N}";

            await using var serviceProvider = TestServiceProviderFactory.Create(
                _fixture.ConnectionString,
                databaseName);

            await serviceProvider.InitializeInfrastructureAsync();

            var client = new MongoClient(_fixture.ConnectionString);
            var database = client.GetDatabase(databaseName);

            var customerIndexesCursor = await database.GetCollection<BsonDocument>("customers").Indexes.ListAsync();
            var customerIndexes = await customerIndexesCursor.ToListAsync();
            customerIndexes.ShouldContain(x => x["name"] == "UX_Customer_ExternalId");
            customerIndexes.ShouldContain(x => x["name"] == "IX_Customer_Companies_TaxId");

            var customersHistoryIndexesCursor = await database.GetCollection<BsonDocument>("customers-history").Indexes.ListAsync();
            var customersHistoryIndexes = await customersHistoryIndexesCursor.ToListAsync();
            customersHistoryIndexes.ShouldContain(x => x["name"] == "IX_CustomersHistory_CustomerId");

            var adminIndexesCursor = await database.GetCollection<BsonDocument>("admins").Indexes.ListAsync();
            var adminIndexes = await adminIndexesCursor.ToListAsync();
            adminIndexes.ShouldContain(x => x["name"] == "UX_Admin_ExternalId");

            var adminHistoryIndexesCursor = await database.GetCollection<BsonDocument>("admins-history").Indexes.ListAsync();
            var adminHistoryIndexes = await adminHistoryIndexesCursor.ToListAsync();
            adminHistoryIndexes.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task InitializeInfrastructureAsync_RepeatedStartPreservesDocumentsAndIndexDefinitions()
        {
            var databaseName = $"user-tests-{Guid.NewGuid():N}";
            var client = new MongoClient(_fixture.ConnectionString);
            var database = client.GetDatabase(databaseName);

            try
            {
                await using var firstHost = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
                await firstHost.InitializeInfrastructureAsync();

                var customer = new BsonDocument
                {
                    ["_id"] = ObjectId.GenerateNewId(),
                    ["ExternalId"] = "existing-customer"
                };
                var history = new BsonDocument
                {
                    ["_id"] = ObjectId.GenerateNewId(),
                    ["CustomerId"] = Guid.NewGuid().ToString()
                };
                await database.GetCollection<BsonDocument>("customers").InsertOneAsync(customer);
                await database.GetCollection<BsonDocument>("customers-history").InsertOneAsync(history);
                var indexesBefore = await SnapshotIndexesAsync(database);

                await firstHost.InitializeInfrastructureAsync();
                await using var secondHost = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
                await secondHost.InitializeInfrastructureAsync();

                (await SnapshotIndexesAsync(database)).SequenceEqual(indexesBefore).ShouldBeTrue();
                (await database.GetCollection<BsonDocument>("customers").Find(FilterDefinition<BsonDocument>.Empty).SingleAsync())
                    .ShouldBe(customer);
                (await database.GetCollection<BsonDocument>("customers-history").Find(FilterDefinition<BsonDocument>.Empty).SingleAsync())
                    .ShouldBe(history);
            }
            finally
            {
                await client.DropDatabaseAsync(databaseName);
            }
        }

        [Fact]
        public async Task InitializeInfrastructureAsync_ConcurrentHostsKeepUniqueIndexesAndData()
        {
            var databaseName = $"user-tests-{Guid.NewGuid():N}";
            var client = new MongoClient(_fixture.ConnectionString);
            var database = client.GetDatabase(databaseName);

            try
            {
                var customer = new BsonDocument
                {
                    ["_id"] = ObjectId.GenerateNewId(),
                    ["ExternalId"] = "existing-customer"
                };
                await database.GetCollection<BsonDocument>("customers").InsertOneAsync(customer);

                await using var firstHost = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
                await using var secondHost = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
                await Task.WhenAll(
                    firstHost.InitializeInfrastructureAsync(),
                    secondHost.InitializeInfrastructureAsync());

                var indexes = await SnapshotIndexesAsync(database);
                indexes.Count.ShouldBe(11); // _id plus application indexes across five collections.
                var customerIndexes = await ListIndexesAsync(database, "customers");
                customerIndexes.Single(x => x["name"] == "UX_Customer_ExternalId")["unique"].AsBoolean.ShouldBeTrue();
                var favoritesIndexes = await ListIndexesAsync(database, "favorites");
                favoritesIndexes.Single(x => x["name"] == "UX_Favorite_ClientId_ProductId")["unique"].AsBoolean.ShouldBeTrue();

                (await database.GetCollection<BsonDocument>("customers").Find(FilterDefinition<BsonDocument>.Empty).SingleAsync())
                    .ShouldBe(customer);
                var duplicate = new BsonDocument { ["_id"] = ObjectId.GenerateNewId(), ["ExternalId"] = "existing-customer" };
                await Should.ThrowAsync<MongoWriteException>(() =>
                    database.GetCollection<BsonDocument>("customers").InsertOneAsync(duplicate));
            }
            finally
            {
                await client.DropDatabaseAsync(databaseName);
            }
        }

        [Fact]
        public async Task InitializeInfrastructureAsync_ConflictingNamedIndexFailsWithoutReplacingItOrDeletingData()
        {
            var databaseName = $"user-tests-{Guid.NewGuid():N}";
            var client = new MongoClient(_fixture.ConnectionString);
            var database = client.GetDatabase(databaseName);
            var customers = database.GetCollection<BsonDocument>("customers");

            try
            {
                var customer = new BsonDocument
                {
                    ["_id"] = ObjectId.GenerateNewId(),
                    ["ExternalId"] = "existing-customer"
                };
                await customers.InsertOneAsync(customer);
                await customers.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("ExternalId"),
                    new CreateIndexOptions { Name = "UX_Customer_ExternalId" }));

                await using var host = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
                var exception = await Should.ThrowAsync<MongoCommandException>(() => host.InitializeInfrastructureAsync());
                new[] { 85, 86 }.ShouldContain(exception.Code); // IndexOptionsConflict or IndexKeySpecsConflict.

                (await customers.Find(FilterDefinition<BsonDocument>.Empty).SingleAsync()).ShouldBe(customer);
                var indexes = await ListIndexesAsync(database, "customers");
                var conflicting = indexes.Single(x => x["name"] == "UX_Customer_ExternalId");
                (conflicting.TryGetValue("unique", out var unique) && unique.AsBoolean).ShouldBeFalse();
            }
            finally
            {
                await client.DropDatabaseAsync(databaseName);
            }
        }

        private static async Task<IReadOnlyList<string>> SnapshotIndexesAsync(IMongoDatabase database)
        {
            var snapshot = new List<string>();
            foreach (var collection in new[] { "customers", "customers-history", "admins", "admins-history", "favorites" })
            {
                var indexes = await ListIndexesAsync(database, collection);
                snapshot.AddRange(indexes.Select(index => $"{collection}/{index["name"]}:{index.ToJson()}"));
            }

            return snapshot.OrderBy(index => index, StringComparer.Ordinal).ToArray();
        }

        private static async Task<IReadOnlyList<BsonDocument>> ListIndexesAsync(IMongoDatabase database, string collection)
        {
            using var cursor = await database.GetCollection<BsonDocument>(collection).Indexes.ListAsync();
            return await cursor.ToListAsync();
        }
    }
}

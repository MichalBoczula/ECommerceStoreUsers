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
            // arrange
            var databaseName = $"user-tests-{Guid.NewGuid():N}";

            await using var serviceProvider = TestServiceProviderFactory.Create(
                _fixture.ConnectionString,
                databaseName);

            // act
            await serviceProvider.InitializeInfrastructureAsync();

            // assert
            var client = new MongoClient(_fixture.ConnectionString);
            var database = client.GetDatabase(databaseName);

            var customerIndexesCursor = await database.GetCollection<BsonDocument>("customers").Indexes.ListAsync();
            var customerIndexes = await customerIndexesCursor.ToListAsync();
            customerIndexes.ShouldContain(x => x["name"] == "UX_Customer_ExternalId");
            customerIndexes.ShouldContain(x => x["name"] == "IX_Customer_Companies_TaxId");

            var adminIndexesCursor = await database.GetCollection<BsonDocument>("admins").Indexes.ListAsync();
            var adminIndexes = await adminIndexesCursor.ToListAsync();
            adminIndexes.ShouldContain(x => x["name"] == "UX_Admin_ExternalId");
        }
    }
}
namespace ECommerceStoreUsers.Infrastructure.Configuration
{
    internal sealed record MongoDbSettings
    {
        public const string SectionName = "MongoDbSettings";
        public required string ConnectionString { get; init; }
        public required string DatabaseName { get; init; }
        public required string CustomerCollectionName { get; init; }
        public required string CustomerHistoryCollectionName { get; init; }
        public required string AdminCollectionName { get; init; }
    }
}

using ECommerceStoreUsers.Infrastructure.Persistance.Customers.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerceStoreUsers.Infrastructure.Persistance.Customers
{
    internal sealed record CustomerDocument
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public required Guid Id { get; init; }

        [BsonRepresentation(BsonType.String)]
        public required string ExternalId { get; init; }

        public required IndividualDataDocument Individual { get; init; }

        public required IReadOnlyCollection<CompanyDataDocument> Companies { get; init; }

        public required DateTime UpdatedAt { get; init; }
    }
}

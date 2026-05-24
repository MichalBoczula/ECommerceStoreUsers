using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerceStoreUsers.Infrastructure.Persistance.Admins
{
    internal sealed record AdminDocument
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public required Guid Id { get; init; }

        public required string ExternalId { get; init; }

        public required string FullName { get; init; }

        public required string Email { get; init; }

        public required bool IsActive { get; init; }

        public required DateTime LastLoginAt { get; init; }
    }
}
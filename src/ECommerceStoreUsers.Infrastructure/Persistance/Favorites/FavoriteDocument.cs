using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerceStoreUsers.Infrastructure.Persistance.Favorites
{
    internal sealed record FavoriteDocument
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public required Guid Id { get; init; }

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public required Guid ClientId { get; init; }

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public required Guid ProductId { get; init; }

        public required DateTime AddedAt { get; init; }
    }
}
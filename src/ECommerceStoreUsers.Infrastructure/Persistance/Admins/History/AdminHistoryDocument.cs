using ECommerceStoreUsers.Domain.Common.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerceStoreUsers.Infrastructure.Persistance.Admins.History
{
    internal sealed class AdminHistoryDocument
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; }
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid AdminId { get; set; }
        public string ExternalId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLoginAt { get; set; }
        public DateTime ChangedAt { get; set; }
        public ActionType Action { get; set; }
    }
}

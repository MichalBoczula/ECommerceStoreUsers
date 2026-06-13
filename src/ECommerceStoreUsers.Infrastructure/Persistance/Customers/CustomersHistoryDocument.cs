using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerceStoreUsers.Infrastructure.Persistance.Customers
{
    internal sealed class CustomersHistoryDocument
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; }

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid CustomerId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public required string ExternalId { get; set; }

        public required IndividualDataDocument Individual { get; set; }

        public required IReadOnlyCollection<CompanyDataDocument> Companies { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime ChangedAt { get; set; }

        public ActionType Action { get; set; }
    }
}

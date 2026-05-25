using ECommerceStoreUsers.Infrastructure.Persistance.Customers.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerceStoreUsers.Infrastructure.Persistance.Customers.Entities
{
    internal sealed record CompanyDataDocument
    {
        [BsonRepresentation(BsonType.String)]
        public required string TaxId { get; init; }
        public required string CompanyName { get; init; }
        public required AddressDocument BillingAddress { get; init; }
        public required AddressDocument ShippingAddress { get; init; }
    }
}

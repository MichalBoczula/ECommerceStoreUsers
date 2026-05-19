using ECommerceStoreUsers.Infrastructure.Persistance.Customers.ValueObjects;

namespace ECommerceStoreUsers.Infrastructure.Persistance.Customers.Entities
{
    internal sealed record IndividualDataDocument
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Email { get; init; }
        public required string Phone { get; init; }
        public required AddressDocument BillingAddress { get; init; }
        public required AddressDocument ShippingAddress { get; init; }
    }
}

using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;

namespace ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities
{
    public sealed record IndividualData(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    Address BillingAddress,
    Address ShippingAddress);
}

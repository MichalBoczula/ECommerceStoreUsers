using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;

namespace ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities
{
    public sealed record CompanyData(
    Guid InternalId,
    string CompanyName,
    string TaxId,
    Address BillingAddress,
    Address ShippingAddress);
}

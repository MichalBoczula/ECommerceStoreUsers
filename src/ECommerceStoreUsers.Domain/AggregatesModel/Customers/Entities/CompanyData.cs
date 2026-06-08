using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;

namespace ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities
{
    public sealed record CompanyData
    {
        public Guid Id { get; private set; }
        public string TaxId { get; private set; }
        public string CompanyName { get; private set; }
        public Address BillingAddress { get; private set; }
        public Address ShippingAddress { get; private set; }

        public CompanyData(
            string taxId,
            string companyName,
            Address billingAddress,
            Address shippingAddress)
        {
            Id = Guid.NewGuid();
            TaxId = taxId;
            CompanyName = companyName;
            BillingAddress = billingAddress;
            ShippingAddress = shippingAddress;
        }

        public void UpdateCompanyDetails(string taxId, string companyName, Address billingAddress, Address shippingAddress)
        {
            TaxId = taxId;
            CompanyName = companyName;
            BillingAddress = billingAddress;
            ShippingAddress = shippingAddress;
        }

        public static CompanyData Rehydrate(
            Guid id,
            string taxId,
            string companyName,
            Address billingAddress,
            Address shippingAddress)
        {
            return new CompanyData(taxId, companyName, billingAddress, shippingAddress)
            {
                Id = id
            };
        }
    }
}

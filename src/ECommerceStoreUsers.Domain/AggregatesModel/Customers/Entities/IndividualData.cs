using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;

namespace ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities
{
    public sealed record IndividualData
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public Address BillingAddress { get; private set; }
        public Address ShippingAddress { get; private set; }

        public IndividualData(
            string firstName,
            string lastName,
            string email,
            string phone,
            Address billingAddress,
            Address shippingAddress)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            BillingAddress = billingAddress;
            ShippingAddress = shippingAddress;
        }

        public void UpdateIndividualData(
            string firstName,
            string lastName,
            string email,
            string phone,
            Address billingAddress,
            Address shippingAddress)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            BillingAddress = billingAddress;
            ShippingAddress = shippingAddress;
        }
    }
}

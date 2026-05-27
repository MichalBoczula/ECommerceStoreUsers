using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Domain
{
    internal static class CustomersValidationDataFactory
    {
        public static Customer CreateValid()
        {
            var address = new Address("00-001", "Warsaw", "Main Street", "10", "2");
            var individual = new IndividualData(
                firstName: "Jan",
                lastName: "Kowalski",
                email: "jan.kowalski@store.com",
                phone: "123456789",
                billingAddress: address,
                shippingAddress: address);

            return new Customer(
                externalId: "entra-id|9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
                individual: individual);
        }

        public static Customer CreateInvalidExternalId()
        {
            var address = new Address("00-001", "Warsaw", "Main Street", "10", "2");
            var individual = new IndividualData(
                firstName: "Jan",
                lastName: "Kowalski",
                email: "jan.kowalski@store.com",
                phone: "123456789",
                billingAddress: address,
                shippingAddress: address);

            return new Customer(
                externalId: " ",
                individual: individual);
        }

        public static Customer CreateAllInvalid()
        {
            var invalidAddress = new Address("00-AB1", "", "", "", "###");
            var invalidIndividual = new IndividualData(
                firstName: "",
                lastName: "",
                email: "bad@@email",
                phone: "123",
                billingAddress: invalidAddress,
                shippingAddress: invalidAddress);

            var invalidCompanies = new List<CompanyData>
            {
                new(
                    taxId: "12",
                    companyName: "",
                    billingAddress: invalidAddress,
                    shippingAddress: invalidAddress)
            };

            return Customer.Rehydrate(
                id: Guid.NewGuid(),
                externalId: " ",
                individual: invalidIndividual,
                companies: invalidCompanies);
        }
    }
}

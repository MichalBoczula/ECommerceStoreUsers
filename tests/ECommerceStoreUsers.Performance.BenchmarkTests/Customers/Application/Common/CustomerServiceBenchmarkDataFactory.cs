using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Application.Common
{
    internal static class CustomerServiceBenchmarkDataFactory
    {
        public static CreateCustomerRequestDto CreateRequest()
        {
            return new CreateCustomerRequestDto
            {
                ExternalId = "auth0|customer-999",
                Individual = new IndividualDataRequestDto
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@store.com",
                    Phone = "+1-555-0100",
                    BillingAddress = CreateAddressRequest("100"),
                    ShippingAddress = CreateAddressRequest("101")
                }
            };
        }

        public static Customer CreateDomainCustomer(Guid id, string externalId)
        {
            var individualData = new IndividualData(
                "John",
                "Doe",
                "john.doe@store.com",
                "+1-555-0100",
                CreateAddress("100"),
                CreateAddress("101"));

            var companies = new List<CompanyData>
            {
                new(
                    "1234567890",
                    "Doe Logistics LLC",
                    CreateAddress("200"),
                    CreateAddress("201"))
            };

            return Customer.Rehydrate(id, externalId, individualData, companies);
        }

        private static AddressRequestDto CreateAddressRequest(string buildingNumber)
        {
            return new AddressRequestDto
            {
                PostalCode = "00-001",
                City = "New York",
                Street = "Main Street",
                BuildingNumber = buildingNumber,
                ApartmentNumber = "10A"
            };
        }

        private static Address CreateAddress(string buildingNumber)
        {
            return new Address(
                "00-001",
                "New York",
                "Main Street",
                buildingNumber,
                "10A");
        }
    }
}

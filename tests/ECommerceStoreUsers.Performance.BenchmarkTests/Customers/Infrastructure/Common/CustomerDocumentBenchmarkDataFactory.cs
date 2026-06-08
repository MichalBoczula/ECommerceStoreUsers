using ECommerceStoreUsers.Infrastructure.Persistance.Customers;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers.Entities;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers.ValueObjects;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Infrastructure.Common
{
    internal static class CustomerDocumentBenchmarkDataFactory
    {
        private static readonly DateTime BenchmarkDate = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        public static CustomerDocument Create(Guid id, string externalId)
        {
            return new CustomerDocument
            {
                Id = id,
                ExternalId = externalId,
                UpdatedAt = BenchmarkDate,
                Individual = new IndividualDataDocument
                {
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    Email = "jan.kowalski@store.com",
                    Phone = "123456789",
                    BillingAddress = new AddressDocument
                    {
                        PostalCode = "00-100",
                        City = "Warsaw",
                        Street = "Main",
                        BuildingNumber = "1",
                        ApartmentNumber = "10"
                    },
                    ShippingAddress = new AddressDocument
                    {
                        PostalCode = "00-200",
                        City = "Krakow",
                        Street = "Market",
                        BuildingNumber = "2",
                        ApartmentNumber = "20"
                    }
                },
                Companies =
                [
                    new CompanyDataDocument
                    {
                        Id = Guid.NewGuid(),
                        TaxId = "1234567890",
                        CompanyName = "Contoso",
                        BillingAddress = new AddressDocument
                        {
                            PostalCode = "55-555",
                            City = "Wroclaw",
                            Street = "Legnicka",
                            BuildingNumber = "2",
                            ApartmentNumber = "3"
                        },
                        ShippingAddress = new AddressDocument
                        {
                            PostalCode = "66-666",
                            City = "Szczecin",
                            Street = "Morska",
                            BuildingNumber = "4",
                            ApartmentNumber = null
                        }
                    }
                ]
            };
        }

        public static CustomerDocument Create() => Create(Guid.NewGuid(), $"entra-id|{Guid.NewGuid()}");
    }
}

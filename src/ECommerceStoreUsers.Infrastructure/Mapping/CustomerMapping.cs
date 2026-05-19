using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers.Entities;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers.ValueObjects;

namespace ECommerceStoreUsers.Infrastructure.Mapping
{
    internal static class CustomerMapping
    {
        internal static CustomerDocument MapToDocument(Customer customer)
        {
            return new CustomerDocument
            {
                Id = customer.Id,
                ExternalId = customer.ExternalId,
                UpdatedAt = customer.UpdatedAt,
                Individual = MapIndividualToDocument(customer.Individual),
                Companies = customer.Companies.Select(MapCompanyToDocument).ToList()
            };
        }

        internal static Customer MapToDomain(CustomerDocument customerDocument)
        {
            var individual = new IndividualData(
                customerDocument.Individual.FirstName,
                customerDocument.Individual.LastName,
                customerDocument.Individual.Email,
                customerDocument.Individual.Phone,
                MapAddressToDomain(customerDocument.Individual.BillingAddress),
                MapAddressToDomain(customerDocument.Individual.ShippingAddress)
            );

            var companies = customerDocument.Companies.Select(x =>
                new CompanyData(
                    x.TaxId,
                    x.CompanyName,
                    MapAddressToDomain(x.BillingAddress),
                    MapAddressToDomain(x.ShippingAddress)
                )).ToList();

            return Customer.Rehydrate(
                customerDocument.Id,
                customerDocument.ExternalId,
                individual,
                companies);
        }

        private static IndividualDataDocument MapIndividualToDocument(IndividualData individual)
        {
            return new IndividualDataDocument
            {
                FirstName = individual.FirstName,
                LastName = individual.LastName,
                Email = individual.Email,
                Phone = individual.Phone,
                BillingAddress = MapAddressToDocument(individual.BillingAddress),
                ShippingAddress = MapAddressToDocument(individual.ShippingAddress)
            };
        }

        private static CompanyDataDocument MapCompanyToDocument(CompanyData company)
        {
            return new CompanyDataDocument
            {
                TaxId = company.TaxId,
                CompanyName = company.CompanyName,
                BillingAddress = MapAddressToDocument(company.BillingAddress),
                ShippingAddress = MapAddressToDocument(company.ShippingAddress)
            };
        }

        private static AddressDocument MapAddressToDocument(Address address)
        {
            return new AddressDocument
            {
                PostalCode = address.PostalCode,
                City = address.City,
                Street = address.Street,
                BuildingNumber = address.BuildingNumber,
                ApartmentNumber = address.ApartmentNumber
            };
        }

        private static Address MapAddressToDomain(AddressDocument doc)
        {
            return new Address(
                doc.PostalCode,
                doc.City,
                doc.Street,
                doc.BuildingNumber,
                doc.ApartmentNumber
            );
        }
    }
}

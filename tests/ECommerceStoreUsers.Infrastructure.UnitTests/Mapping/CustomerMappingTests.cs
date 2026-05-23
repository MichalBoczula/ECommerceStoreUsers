using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers.Entities;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers.ValueObjects;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Mapping;

public sealed class CustomerMappingTests
{
    [Fact]
    public void MapToDocument_ShouldMapAllCustomerFields()
    {
        var customer = CreateCustomer();

        var document = CustomerMapping.MapToDocument(customer);

        Assert.Equal(customer.Id, document.Id);
        Assert.Equal(customer.ExternalId, document.ExternalId);
        Assert.Equal(customer.UpdatedAt, document.UpdatedAt);
        Assert.Equal(customer.Individual.FirstName, document.Individual.FirstName);
        Assert.Equal(customer.Individual.ShippingAddress.PostalCode, document.Individual.ShippingAddress.PostalCode);
        Assert.Single(document.Companies);
        Assert.Equal(customer.Companies.Single().CompanyName, document.Companies.Single().CompanyName);
        Assert.Equal(customer.Companies.Single().BillingAddress.City, document.Companies.Single().BillingAddress.City);
    }

    [Fact]
    public void MapToDomain_ShouldMapAllCustomerFields()
    {
        var document = CreateCustomerDocument();

        var domain = CustomerMapping.MapToDomain(document);

        Assert.Equal(document.Id, domain.Id);
        Assert.Equal(document.ExternalId, domain.ExternalId);
        Assert.Equal(document.Individual.Email, domain.Individual.Email);
        Assert.Equal(document.Individual.BillingAddress.Street, domain.Individual.BillingAddress.Street);
        Assert.Single(domain.Companies);
        Assert.Equal(document.Companies.Single().TaxId, domain.Companies.Single().TaxId);
        Assert.Equal(document.Companies.Single().ShippingAddress.City, domain.Companies.Single().ShippingAddress.City);
    }

    [Fact]
    public void MapIndividualToDocument_ShouldMapAllFields()
    {
        var individual = CreateIndividual();

        var document = CustomerMapping.MapIndividualToDocument(individual);

        Assert.Equal(individual.FirstName, document.FirstName);
        Assert.Equal(individual.LastName, document.LastName);
        Assert.Equal(individual.Email, document.Email);
        Assert.Equal(individual.Phone, document.Phone);
        Assert.Equal(individual.BillingAddress.PostalCode, document.BillingAddress.PostalCode);
        Assert.Equal(individual.ShippingAddress.BuildingNumber, document.ShippingAddress.BuildingNumber);
    }

    [Fact]
    public void MapCompanyToDocument_ShouldMapAllFields()
    {
        var company = CreateCompany();

        var document = CustomerMapping.MapCompanyToDocument(company);

        Assert.Equal(company.TaxId, document.TaxId);
        Assert.Equal(company.CompanyName, document.CompanyName);
        Assert.Equal(company.BillingAddress.City, document.BillingAddress.City);
        Assert.Equal(company.ShippingAddress.ApartmentNumber, document.ShippingAddress.ApartmentNumber);
    }

    [Fact]
    public void MapAddressToDocument_ShouldMapAllFields()
    {
        var address = new Address("77-777", "Gdansk", "Dluga", "22", null);

        var document = CustomerMapping.MapAddressToDocument(address);

        Assert.Equal(address.PostalCode, document.PostalCode);
        Assert.Equal(address.City, document.City);
        Assert.Equal(address.Street, document.Street);
        Assert.Equal(address.BuildingNumber, document.BuildingNumber);
        Assert.Equal(address.ApartmentNumber, document.ApartmentNumber);
    }

    [Fact]
    public void MapAddressToDomain_ShouldMapAllFields()
    {
        var document = new AddressDocument
        {
            PostalCode = "88-888",
            City = "Poznan",
            Street = "Polna",
            BuildingNumber = "10",
            ApartmentNumber = "5"
        };

        var address = CustomerMapping.MapAddressToDomain(document);

        Assert.Equal(document.PostalCode, address.PostalCode);
        Assert.Equal(document.City, address.City);
        Assert.Equal(document.Street, address.Street);
        Assert.Equal(document.BuildingNumber, address.BuildingNumber);
        Assert.Equal(document.ApartmentNumber, address.ApartmentNumber);
    }

    private static Customer CreateCustomer()
    {
        var customer = new Customer("external-1", CreateIndividual());
        customer.AddCompany(
            "Contoso",
            "1234567890",
            new Address("55-555", "Wroclaw", "Legnicka", "2", "3"),
            new Address("66-666", "Szczecin", "Morska", "4", null));

        return customer;
    }

    private static IndividualData CreateIndividual() => new(
        "Jan",
        "Kowalski",
        "jan.kowalski@example.com",
        "123456789",
        new Address("00-100", "Warsaw", "Main", "1", "10"),
        new Address("00-200", "Krakow", "Market", "2", "20"));

    private static CompanyData CreateCompany() => new(
        "1234567890",
        "Contoso",
        new Address("55-555", "Wroclaw", "Legnicka", "2", "3"),
        new Address("66-666", "Szczecin", "Morska", "4", null));

    private static CustomerDocument CreateCustomerDocument() => new()
    {
        Id = Guid.NewGuid(),
        ExternalId = "external-2",
        UpdatedAt = new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc),
        Individual = new IndividualDataDocument
        {
            FirstName = "Anna",
            LastName = "Nowak",
            Email = "anna.nowak@example.com",
            Phone = "987654321",
            BillingAddress = new AddressDocument
            {
                PostalCode = "11-111",
                City = "Lodz",
                Street = "Piotrkowska",
                BuildingNumber = "7",
                ApartmentNumber = "8"
            },
            ShippingAddress = new AddressDocument
            {
                PostalCode = "22-222",
                City = "Bydgoszcz",
                Street = "Lesna",
                BuildingNumber = "9",
                ApartmentNumber = null
            }
        },
        Companies =
        [
            new CompanyDataDocument
            {
                TaxId = "1112223334",
                CompanyName = "Fabrikam",
                BillingAddress = new AddressDocument
                {
                    PostalCode = "33-333",
                    City = "Katowice",
                    Street = "Sloneczna",
                    BuildingNumber = "10",
                    ApartmentNumber = "11"
                },
                ShippingAddress = new AddressDocument
                {
                    PostalCode = "44-444",
                    City = "Gdynia",
                    Street = "Morska",
                    BuildingNumber = "12",
                    ApartmentNumber = "13"
                }
            }
        ]
    };
}

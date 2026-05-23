using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;

namespace ECommerceStoreUsers.Application.UnitTests.Mapping;

public class MappingConfigTests
{
    [Fact]
    public void MapToDomain_CreateCustomerRequestDto_ShouldMapCustomerWithIndividual()
    {
        // Arrange
        var request = new CreateCustomerRequestDto
        {
            ExternalId = "external-1",
            Individual = CreateIndividualRequestDto()
        };

        // Act
        var result = MappingConfig.MapToDomain(request);

        // Assert
        Assert.Equal("external-1", result.ExternalId);
        Assert.Equal("Jan", result.Individual.FirstName);
        Assert.Equal("Kowalski", result.Individual.LastName);
        Assert.Equal("jan.kowalski@example.com", result.Individual.Email);
        Assert.Equal("123456789", result.Individual.Phone);
        Assert.Equal("00-100", result.Individual.BillingAddress.PostalCode);
        Assert.Equal("Warsaw", result.Individual.BillingAddress.City);
        Assert.Equal("Main", result.Individual.BillingAddress.Street);
        Assert.Equal("1", result.Individual.BillingAddress.BuildingNumber);
        Assert.Equal("10", result.Individual.BillingAddress.ApartmentNumber);
        Assert.Equal("00-200", result.Individual.ShippingAddress.PostalCode);
        Assert.Equal("Krakow", result.Individual.ShippingAddress.City);
    }

    [Fact]
    public void MapToDomain_IndividualDataRequestDto_ShouldMapAllFields()
    {
        // Arrange
        var request = CreateIndividualRequestDto();

        // Act
        var result = MappingConfig.MapToDomain(request);

        // Assert
        Assert.Equal(request.FirstName, result.FirstName);
        Assert.Equal(request.LastName, result.LastName);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Phone, result.Phone);
        Assert.Equal(request.BillingAddress.PostalCode, result.BillingAddress.PostalCode);
        Assert.Equal(request.BillingAddress.City, result.BillingAddress.City);
        Assert.Equal(request.BillingAddress.Street, result.BillingAddress.Street);
        Assert.Equal(request.BillingAddress.BuildingNumber, result.BillingAddress.BuildingNumber);
        Assert.Equal(request.BillingAddress.ApartmentNumber, result.BillingAddress.ApartmentNumber);
        Assert.Equal(request.ShippingAddress.PostalCode, result.ShippingAddress.PostalCode);
        Assert.Equal(request.ShippingAddress.City, result.ShippingAddress.City);
        Assert.Equal(request.ShippingAddress.Street, result.ShippingAddress.Street);
        Assert.Equal(request.ShippingAddress.BuildingNumber, result.ShippingAddress.BuildingNumber);
        Assert.Equal(request.ShippingAddress.ApartmentNumber, result.ShippingAddress.ApartmentNumber);
    }

    [Fact]
    public void MapAddress_ShouldMapAllFields()
    {
        // Arrange
        var request = new AddressRequestDto
        {
            PostalCode = "11-111",
            City = "Gdansk",
            Street = "Dluga",
            BuildingNumber = "22",
            ApartmentNumber = null
        };

        // Act
        var result = MappingConfig.MapAddress(request);

        // Assert
        Assert.Equal(request.PostalCode, result.PostalCode);
        Assert.Equal(request.City, result.City);
        Assert.Equal(request.Street, result.Street);
        Assert.Equal(request.BuildingNumber, result.BuildingNumber);
        Assert.Equal(request.ApartmentNumber, result.ApartmentNumber);
    }

    [Fact]
    public void MapToResponse_Customer_ShouldMapNestedObjectsAndCompanies()
    {
        // Arrange
        var individual = new IndividualData(
            "Anna",
            "Nowak",
            "anna.nowak@example.com",
            "987654321",
            new Address("33-333", "Lodz", "Piotrkowska", "7", "8"),
            new Address("44-444", "Poznan", "Polna", "9", null));

        var customer = new Customer("ext-2", individual);
        customer.AddCompany(
            "Contoso",
            "1234567890",
            new Address("55-555", "Wroclaw", "Legnicka", "2", "3"),
            new Address("66-666", "Szczecin", "Morska", "4", null));

        // Act
        var result = MappingConfig.MapToResponse(customer);

        // Assert
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal(customer.ExternalId, result.ExternalId);
        Assert.Equal(customer.UpdatedAt, result.UpdatedAt);
        Assert.Equal("Anna", result.Individual.FirstName);
        Assert.Equal("Nowak", result.Individual.LastName);
        Assert.Equal("anna.nowak@example.com", result.Individual.Email);
        Assert.Equal(1, result.Companies.Count);

        var company = result.Companies.Single();
        Assert.Equal("Contoso", company.CompanyName);
        Assert.Equal("1234567890", company.TaxId);
        Assert.Equal("55-555", company.BillingAddress.PostalCode);
        Assert.Equal("66-666", company.ShippingAddress.PostalCode);
    }

    [Fact]
    public void MapToResponse_IndividualData_ShouldMapAllFields()
    {
        // Arrange
        var individual = new IndividualData(
            "Tom",
            "Smith",
            "tom.smith@example.com",
            "555333111",
            new Address("70-700", "Bydgoszcz", "Leśna", "12", "1"),
            new Address("80-800", "Torun", "Parkowa", "13", null));

        // Act
        var result = MappingConfig.MapToResponse(individual);

        // Assert
        Assert.Equal(individual.FirstName, result.FirstName);
        Assert.Equal(individual.LastName, result.LastName);
        Assert.Equal(individual.Email, result.Email);
        Assert.Equal(individual.Phone, result.Phone);
        Assert.Equal(individual.BillingAddress.PostalCode, result.BillingAddress.PostalCode);
        Assert.Equal(individual.ShippingAddress.PostalCode, result.ShippingAddress.PostalCode);
    }

    [Fact]
    public void MapToResponse_CompanyData_ShouldMapAllFields()
    {
        // Arrange
        var company = new CompanyData(
            "5554443332",
            "Fabrikam",
            new Address("90-900", "Opole", "Krotka", "5", "2"),
            new Address("91-901", "Olsztyn", "Dluga", "6", null));

        // Act
        var result = MappingConfig.MapToResponse(company);

        // Assert
        Assert.Equal(company.Id, result.Id);
        Assert.Equal(company.TaxId, result.TaxId);
        Assert.Equal(company.CompanyName, result.CompanyName);
        Assert.Equal(company.BillingAddress.City, result.BillingAddress.City);
        Assert.Equal(company.ShippingAddress.City, result.ShippingAddress.City);
    }

    [Fact]
    public void MapToResponse_Address_ShouldMapAllFields()
    {
        // Arrange
        var address = new Address("12-345", "Katowice", "Słoneczna", "77", "12");

        // Act
        var result = MappingConfig.MapToResponse(address);

        // Assert
        Assert.Equal(address.PostalCode, result.PostalCode);
        Assert.Equal(address.City, result.City);
        Assert.Equal(address.Street, result.Street);
        Assert.Equal(address.BuildingNumber, result.BuildingNumber);
        Assert.Equal(address.ApartmentNumber, result.ApartmentNumber);
    }

    private static IndividualDataRequestDto CreateIndividualRequestDto()
    {
        return new IndividualDataRequestDto
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan.kowalski@example.com",
            Phone = "123456789",
            BillingAddress = new AddressRequestDto
            {
                PostalCode = "00-100",
                City = "Warsaw",
                Street = "Main",
                BuildingNumber = "1",
                ApartmentNumber = "10"
            },
            ShippingAddress = new AddressRequestDto
            {
                PostalCode = "00-200",
                City = "Krakow",
                Street = "Market",
                BuildingNumber = "2",
                ApartmentNumber = "20"
            }
        };
    }
}

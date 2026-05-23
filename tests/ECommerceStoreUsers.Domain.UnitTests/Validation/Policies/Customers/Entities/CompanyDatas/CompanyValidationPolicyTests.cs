using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Customers.Entities.CompanyDatas;

public class CompanyValidationPolicyTests
{
    private readonly CompanyValidationPolicy _policy = new();

    [Fact]
    public async Task Validate_HappyPath_ShouldReturnNoErrors()
    {
        // Arrange
        var company = CreateCompany(
            taxId: "1234567890",
            companyName: "Acme Sp. z o.o.",
            billingAddress: CreateAddress("00-001", "Warsaw", "Main St", "10", "5"),
            shippingAddress: CreateAddress("00-002", "Krakow", "Side St", "20", "7"));

        // Act
        var result = await _policy.Validate(company);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.GetValidationErrors().Count.ShouldBe(0);
    }

    [Fact]
    public async Task Validate_AllErrors_ShouldReturnAllValidationErrors()
    {
        // Arrange
        var invalidAddress = CreateAddress("", "", "", "", "");
        var company = CreateCompany("ABC", "", invalidAddress, invalidAddress);

        // Act
        var result = await _policy.Validate(company);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(12);
        result.GetValidationErrors().ShouldContain(e => e.Name == "CompanyDataCompanyNameValidationRule");
        result.GetValidationErrors().ShouldContain(e => e.Name == "CompanyDataTaxIdValidationRule");
        result.GetValidationErrors().Count(e => e.Name == "AddressPostalCodeValidationRule").ShouldBe(2);
        result.GetValidationErrors().Count(e => e.Name == "AddressCityValidationRule").ShouldBe(2);
        result.GetValidationErrors().Count(e => e.Name == "AddressStreetValidationRule").ShouldBe(2);
        result.GetValidationErrors().Count(e => e.Name == "AddressBuildingNumberValidationRule").ShouldBe(2);
        result.GetValidationErrors().Count(e => e.Name == "AddressApartmentNumberValidationRule").ShouldBe(2);
    }

    [Fact]
    public async Task Validate_FewErrors_ShouldReturnOnlyRelevantErrors()
    {
        // Arrange
        var validAddress = CreateAddress("00-001", "Warsaw", "Main St", "10", "5");
        var invalidShippingAddress = CreateAddress("00-001", "", "Main St", "", "5");
        var company = CreateCompany("1234567890", "", validAddress, invalidShippingAddress);

        // Act
        var result = await _policy.Validate(company);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(3);
        result.GetValidationErrors().ShouldContain(e => e.Name == "CompanyDataCompanyNameValidationRule");
        result.GetValidationErrors().ShouldContain(e => e.Name == "AddressCityValidationRule");
        result.GetValidationErrors().ShouldContain(e => e.Name == "AddressBuildingNumberValidationRule");
    }

    private static CompanyData CreateCompany(string taxId, string companyName, Address billingAddress, Address shippingAddress)
        => new(taxId, companyName, billingAddress, shippingAddress);

    private static Address CreateAddress(string postalCode, string city, string street, string buildingNumber, string apartmentNumber)
        => new(postalCode, city, street, buildingNumber, apartmentNumber);
}

using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Customers.Entities.CompanyDatas;

public class CompanyDataValidationPolicyTests
{
    private readonly CustomerValidationPolicy _policy = new();

    private const string ValidExternalId = "auth0-user-123";
    private const string ValidFirstName = "Jan";
    private const string ValidLastName = "Kowalski";
    private const string ValidEmail = "jan@kowalski.pl";
    private const string ValidPhone = "123456789";
    private static readonly Address ValidAddress = new("00-001", "Warsaw", "Main St", "10", "5");

    [Fact]
    public async Task Validate_CompanyDataWithInvalidData_ShouldReturnMultipleErrors()
    {
        // Arrange
        var invalidCompany = new CompanyData("123456789A", "", ValidAddress, ValidAddress);

        var individual = new IndividualData(ValidFirstName, ValidLastName, ValidEmail, ValidPhone, ValidAddress, ValidAddress);
        var customer = Customer.Rehydrate(Guid.NewGuid(), ValidExternalId, individual, new List<CompanyData> { invalidCompany });

        // Act
        var result = await _policy.Validate(customer);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(2);
        result.GetValidationErrors().ShouldContain(e => e.Name == "CompanyDataCompanyNameValidationRule");
        result.GetValidationErrors().ShouldContain(e => e.Name == "CompanyDataTaxIdValidationRule");
    }

    [Fact]
    public async Task Validate_CompanyDataIsValid_ShouldReturnNoErrors()
    {
        // Arrange
        var validCompany = new CompanyData("1234567890", "Acme Sp. z o.o.", ValidAddress, ValidAddress);

        var individual = new IndividualData(ValidFirstName, ValidLastName, ValidEmail, ValidPhone, ValidAddress, ValidAddress);
        var customer = Customer.Rehydrate(Guid.NewGuid(), ValidExternalId, individual, new List<CompanyData> { validCompany });

        // Act
        var result = await _policy.Validate(customer);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.GetValidationErrors().Count.ShouldBe(0);
    }

    [Fact]
    public async Task Validate_MultipleCompaniesWithInvalidData_ShouldAccumulateErrorsFromAllCompanies()
    {
        // Arrange
        var firstInvalidCompany = new CompanyData("123456789A", "", ValidAddress, ValidAddress);
        var secondInvalidCompany = new CompanyData("987654321B", "", ValidAddress, ValidAddress);

        var individual = new IndividualData(ValidFirstName, ValidLastName, ValidEmail, ValidPhone, ValidAddress, ValidAddress);
        var customer = Customer.Rehydrate(
            Guid.NewGuid(),
            ValidExternalId,
            individual,
            new List<CompanyData> { firstInvalidCompany, secondInvalidCompany });

        // Act
        var result = await _policy.Validate(customer);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(4);

        var errors = result.GetValidationErrors();
        errors.Count(e => e.Name == "CompanyDataCompanyNameValidationRule").ShouldBe(2);
        errors.Count(e => e.Name == "CompanyDataTaxIdValidationRule").ShouldBe(2);
    }
}
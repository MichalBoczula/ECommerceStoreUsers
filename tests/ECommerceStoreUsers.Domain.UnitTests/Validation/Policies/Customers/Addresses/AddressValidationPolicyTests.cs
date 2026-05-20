using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Customers;

public class CustomerValidationPolicyAddressTests
{
    private readonly CustomerValidationPolicy _policy = new();

    private const string ValidExternalId = "auth0|123456";
    private const string ValidFirstName = "Jan";
    private const string ValidLastName = "Kowalski";
    private const string ValidEmail = "jan@kowalski.pl";
    private const string ValidPhone = "123456789";
    private readonly Address _validAddress = new("00-001", "Warsaw", "Main St", "10", "5");
    private readonly Address _invalidAddress = new("", "", " ", "", "\t");

    [Fact]
    public async Task Validate_IndividualWithInvalidAddresses_ShouldReturnTenErrors()
    {
        // Arrange
        var individual = new IndividualData(ValidFirstName, ValidLastName, ValidEmail, ValidPhone, _invalidAddress, _invalidAddress);
        var customer = Customer.Rehydrate(Guid.NewGuid(), ValidExternalId, individual, new List<CompanyData>());

        // Act
        var result = await _policy.Validate(customer);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(10);
    }

    [Fact]
    public async Task Validate_CompanyWithInvalidAddresses_ShouldReturnTenErrors()
    {
        // Arrange
        var individual = new IndividualData(ValidFirstName, ValidLastName, ValidEmail, ValidPhone, _validAddress, _validAddress);
        var companies = new List<CompanyData>
        {
            new("1234567890", "Test Company LLC", _invalidAddress, _invalidAddress)
        };
        var customer = Customer.Rehydrate(Guid.NewGuid(), ValidExternalId, individual, companies);

        // Act
        var result = await _policy.Validate(customer);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(10);
    }

    [Fact]
    public async Task Validate_MultipleCompaniesWithInvalidAddresses_ShouldAccumulateAllErrors()
    {
        // Arrange
        var individual = new IndividualData(ValidFirstName, ValidLastName, ValidEmail, ValidPhone, _validAddress, _validAddress);
        var companies = new List<CompanyData>
        {
            new("1234567890", "First Corp", _invalidAddress, _invalidAddress),
            new("0987654321", "Second Corp", _invalidAddress, _invalidAddress)
        };
        var customer = Customer.Rehydrate(Guid.NewGuid(), ValidExternalId, individual, companies);

        // Act
        var result = await _policy.Validate(customer);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(20);
    }

    [Fact]
    public async Task Validate_NoCompanyAddresses_ShouldBeValidIfIndividualAddressesAreValid()
    {
        // Arrange
        var individual = new IndividualData(ValidFirstName, ValidLastName, ValidEmail, ValidPhone, _validAddress, _validAddress);
        var customer = Customer.Rehydrate(Guid.NewGuid(), ValidExternalId, individual, new List<CompanyData>());

        // Act
        var result = await _policy.Validate(customer);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.GetValidationErrors().Count.ShouldBe(0);
    }

    [Fact]
    public async Task Validate_OnlyShippingAddressesAreInvalidInBothIndividualAndCompany_ShouldReturnTenErrors()
    {
        // Arrange
        var individual = new IndividualData(ValidFirstName, ValidLastName, ValidEmail, ValidPhone, _validAddress, _invalidAddress);
        var companies = new List<CompanyData>
        {
            new("1234567890", "Shipping Test Sp. z o.o.", _validAddress, _invalidAddress)
        };
        var customer = Customer.Rehydrate(Guid.NewGuid(), ValidExternalId, individual, companies);

        // Act
        var result = await _policy.Validate(customer);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(10);
    }
}
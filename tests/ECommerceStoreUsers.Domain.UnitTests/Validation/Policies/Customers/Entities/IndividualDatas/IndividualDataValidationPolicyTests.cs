using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Customers.Entities.IndividualDatas;

public class IndividualDataValidationPolicyTests
{
    private readonly CustomerValidationPolicy _policy = new();

    private const string ValidExternalId = "auth0-user-123";
    private static readonly Address ValidAddress = new("00-001", "Warsaw", "Main St", "10", "5");

    [Fact]
    public async Task Validate_IndividualDataWithInvalidData_ShouldReturnMultipleErrors()
    {
        var invalidIndividual = new IndividualData("", "123", "invalid-email", "123-456", ValidAddress, ValidAddress);
        var customer = Customer.Rehydrate(Guid.NewGuid(), ValidExternalId, invalidIndividual, new List<CompanyData>());

        // Act
        var result = await _policy.Validate(customer);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(4);
        result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataFirstNameValidationRule");
        result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataLastNameValidationRule");
        result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataEmailValidationRule");
        result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataPhoneValidationRule");
    }

    [Fact]
    public async Task Validate_IndividualDataIsValid_ShouldReturnNoErrors()
    {
        var validIndividual = new IndividualData("John", "Doe", "john.doe@example.com", "1234567", ValidAddress, ValidAddress);
        var customer = Customer.Rehydrate(Guid.NewGuid(), ValidExternalId, validIndividual, new List<CompanyData>());

        // Act
        var result = await _policy.Validate(customer);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.GetValidationErrors().Count.ShouldBe(0);
    }
}
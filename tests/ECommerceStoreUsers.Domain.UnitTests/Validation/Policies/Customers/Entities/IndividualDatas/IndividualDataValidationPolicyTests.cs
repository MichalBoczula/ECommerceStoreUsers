using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Customers.Entities.IndividualDatas;

public class IndividualDataValidationPolicyTests
{
    private readonly IndividualDataValidationPolicy _policy = new();

    [Fact]
    public async Task Validate_HappyPath_ShouldReturnNoErrors()
    {
        var individualData = CreateIndividualData("John", "Doe", "john.doe@example.com", "123456789", CreateAddress("00-001", "Warsaw", "Main Street", "10", "5"), CreateAddress("00-002", "Krakow", "Long Street", "20", "7"));

        var result = await _policy.Validate(individualData);

        result.IsValid.ShouldBeTrue();
        result.GetValidationErrors().Count.ShouldBe(0);
    }

    [Fact]
    public async Task Validate_ErrorPathAll_ShouldReturnAllErrors()
    {
        var individualData = CreateIndividualData("", "", "invalid-email", "123-abc", CreateAddress("", "", "", "", ""), CreateAddress("", "", "", "", ""));

        var result = await _policy.Validate(individualData);

        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(14);
        result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataFirstNameValidationRule");
        result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataLastNameValidationRule");
        result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataEmailValidationRule");
        result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataPhoneValidationRule");
        result.GetValidationErrors().Count(e => e.Name == "AddressPostalCodeValidationRule").ShouldBe(2);
        result.GetValidationErrors().Count(e => e.Name == "AddressCityValidationRule").ShouldBe(2);
        result.GetValidationErrors().Count(e => e.Name == "AddressStreetValidationRule").ShouldBe(2);
        result.GetValidationErrors().Count(e => e.Name == "AddressBuildingNumberValidationRule").ShouldBe(2);
        result.GetValidationErrors().Count(e => e.Name == "AddressApartmentNumberValidationRule").ShouldBe(2);
    }

    [Fact]
    public async Task Validate_OnlyFewError_ShouldReturnOnlySelectedErrors()
    {
        var individualData = CreateIndividualData("John", "Doe", "john.doe@example.com", "123456789", CreateAddress("00-001", "", "Main Street", "10", "5"), CreateAddress("00-002", "Krakow", "Long Street", "", "7"));

        var result = await _policy.Validate(individualData);

        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(2);
        result.GetValidationErrors().ShouldContain(e => e.Name == "AddressCityValidationRule");
        result.GetValidationErrors().ShouldContain(e => e.Name == "AddressBuildingNumberValidationRule");
    }

    private static IndividualData CreateIndividualData(string firstName, string lastName, string email, string phone, Address billingAddress, Address shippingAddress)
        => new(firstName, lastName, email, phone, billingAddress, shippingAddress);

    private static Address CreateAddress(string postalCode, string city, string street, string buildingNumber, string apartmentNumber)
        => new(postalCode, city, street, buildingNumber, apartmentNumber);
}

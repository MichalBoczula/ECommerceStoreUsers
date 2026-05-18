using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers.Addresses;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Customers.Addresses
{
    public class AddressValidationPolicyTests
    {
        [Fact]
        public async Task Validate_AddressWithInvalidData_ShouldReturnMultipleErrors()
        {
            // Arrange
            var policy = new AddressValidationPolicy();
            var address = new Address("", "", " ", "", "\t");

            // Act
            var result = await policy.Validate(address);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.GetValidationErrors().Count.ShouldBe(5);
            result.GetValidationErrors().ShouldContain(e => e.Name == "AddressPostalCodeValidationRule");
            result.GetValidationErrors().ShouldContain(e => e.Name == "AddressCityValidationRule");
            result.GetValidationErrors().ShouldContain(e => e.Name == "AddressStreetValidationRule");
            result.GetValidationErrors().ShouldContain(e => e.Name == "AddressBuildingNumberValidationRule");
            result.GetValidationErrors().ShouldContain(e => e.Name == "AddressApartmentNumberValidationRule");
        }

        [Fact]
        public async Task Validate_AddressIsValid_ShouldReturnNoErrors()
        {
            // Arrange
            var policy = new AddressValidationPolicy();
            var address = new Address("00-001", "Warsaw", "Main St", "10", "5");

            // Act
            var result = await policy.Validate(address);

            // Assert
            result.IsValid.ShouldBeTrue();
            result.GetValidationErrors().Count.ShouldBe(0);
        }
    }
}
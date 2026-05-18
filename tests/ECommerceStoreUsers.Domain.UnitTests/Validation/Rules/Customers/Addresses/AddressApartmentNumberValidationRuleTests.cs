using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Addresses;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Customers.Addresses
{
    public class AddressApartmentNumberValidationRuleTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task IsValid_ApartmentNumberIsNullOrWhiteSpace_ShouldReturnError(string? invalidApartmentNumber)
        {
            // Arrange
            var rule = new AddressApartmentNumberValidationRule();
            var validationResult = new ValidationResult();
            var address = new Address("00-001", "City", "Street", "1", invalidApartmentNumber!);

            // Act
            await rule.IsValid(address, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Apartment Number cannot be empty or white space.");
        }

        [Fact]
        public async Task IsValid_ValidApartmentNumber_ShouldNotReturnError()
        {
            // Arrange
            var rule = new AddressApartmentNumberValidationRule();
            var validationResult = new ValidationResult();
            var address = new Address("00-001", "City", "Street", "1", "12");

            // Act
            await rule.IsValid(address, validationResult);

            // Assert
            validationResult.GetValidationErrors().ShouldBeEmpty();
        }
    }
}
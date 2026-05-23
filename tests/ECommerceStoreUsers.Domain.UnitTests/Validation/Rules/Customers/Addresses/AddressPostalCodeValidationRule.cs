using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.Addresses;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Customers.Addresses
{
    public class AddressPostalCodeValidationRuleTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task IsValid_PostalCodeIsNullOrWhiteSpace_ShouldReturnError(string? invalidPostalCode)
        {
            // Arrange
            var rule = new AddressPostalCodeValidationRule();
            var validationResult = new ValidationResult();
            var address = new Address(invalidPostalCode!, "City", "Street", "1", "1A");

            // Act
            await rule.IsValid(address, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Postal Code cannot be empty or white space.");
        }

        [Fact]
        public async Task IsValid_ValidPostalCode_ShouldNotReturnError()
        {
            // Arrange
            var rule = new AddressPostalCodeValidationRule();
            var validationResult = new ValidationResult();
            var address = new Address("00-001", "City", "Street", "1", "1A");

            // Act
            await rule.IsValid(address, validationResult);

            // Assert
            validationResult.GetValidationErrors().ShouldBeEmpty();
        }
    }
}
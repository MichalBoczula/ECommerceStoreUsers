using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Addresses;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Customers.Addresses
{
    public class AddressCityValidationRuleTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task IsValid_CityIsNullOrWhiteSpace_ShouldReturnError(string? invalidCity)
        {
            // Arrange
            var rule = new AddressCityValidationRule();
            var validationResult = new ValidationResult();
            var address = new Address("00-001", invalidCity!, "Street", "1", "1A");

            // Act
            await rule.IsValid(address, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("City cannot be empty or white space.");
        }

        [Fact]
        public async Task IsValid_ValidCity_ShouldNotReturnError()
        {
            // Arrange
            var rule = new AddressCityValidationRule();
            var validationResult = new ValidationResult();
            var address = new Address("00-001", "Warsaw", "Street", "1", "1A");

            // Act
            await rule.IsValid(address, validationResult);

            // Assert
            validationResult.GetValidationErrors().ShouldBeEmpty();
        }
    }
}
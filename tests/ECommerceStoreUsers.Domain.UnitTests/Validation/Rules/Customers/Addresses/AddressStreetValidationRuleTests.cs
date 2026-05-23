using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.Addresses;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Customers.Addresses
{
    public class AddressStreetValidationRuleTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task IsValid_StreetIsNullOrWhiteSpace_ShouldReturnError(string? invalidStreet)
        {
            // Arrange
            var rule = new AddressStreetValidationRule();
            var validationResult = new ValidationResult();
            var address = new Address("00-001", "City", invalidStreet!, "1", "1A");

            // Act
            await rule.IsValid(address, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Street cannot be empty or white space.");
        }

        [Fact]
        public async Task IsValid_ValidStreet_ShouldNotReturnError()
        {
            // Arrange
            var rule = new AddressStreetValidationRule();
            var validationResult = new ValidationResult();
            var address = new Address("00-001", "City", "Main St", "1", "1A");

            // Act
            await rule.IsValid(address, validationResult);

            // Assert
            validationResult.GetValidationErrors().ShouldBeEmpty();
        }
    }
}
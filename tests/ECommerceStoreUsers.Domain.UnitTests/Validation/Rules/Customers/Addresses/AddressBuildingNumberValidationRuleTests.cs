using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.Addresses;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Customers.Addresses
{
    public class AddressBuildingNumberValidationRuleTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task IsValid_BuildingNumberIsNullOrWhiteSpace_ShouldReturnError(string? invalidBuildingNumber)
        {
            // Arrange
            var rule = new AddressBuildingNumberValidationRule();
            var validationResult = new ValidationResult();
            var address = new Address("00-001", "City", "Street", invalidBuildingNumber!, "1A");

            // Act
            await rule.IsValid(address, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Building Number cannot be empty or white space.");
        }

        [Fact]
        public async Task IsValid_ValidBuildingNumber_ShouldNotReturnError()
        {
            // Arrange
            var rule = new AddressBuildingNumberValidationRule();
            var validationResult = new ValidationResult();
            var address = new Address("00-001", "City", "Street", "10B", "1A");

            // Act
            await rule.IsValid(address, validationResult);

            // Assert
            validationResult.GetValidationErrors().ShouldBeEmpty();
        }
    }
}
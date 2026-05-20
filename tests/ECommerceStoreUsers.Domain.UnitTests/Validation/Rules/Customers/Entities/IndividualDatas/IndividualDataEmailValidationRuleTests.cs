using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Entities.IndividualDatas;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Customers.Entities.IndividualDatas
{
    public class IndividualDataEmailValidationRuleTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("invalid-email")]
        [InlineData("test@domain")]
        [InlineData("@missing-user.com")]
        public async Task IsValid_EmailFormatIsInvalid_ShouldReturnError(string email)
        {
            // Arrange
            var rule = new IndividualDataEmailValidationRule();
            var validationResult = new ValidationResult();
            var individualData = CreateIndividualData(email: email);

            // Act
            await rule.IsValid(individualData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Email must be a valid format (address@domain.something).");
        }

        [Fact]
        public async Task IsValid_EmailFormatIsValid_ShouldReturnNoErrors()
        {
            // Arrange
            var rule = new IndividualDataEmailValidationRule();
            var validationResult = new ValidationResult();
            var individualData = CreateIndividualData(email: "john.doe@example.com");

            // Act
            await rule.IsValid(individualData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        private static IndividualData CreateIndividualData(string email = "john.doe@example.com") =>
            new("John", "Doe", email, "1234567", CreateAddress(), CreateAddress());

        private static Address CreateAddress() => new("00-001", "Warsaw", "Main St", "10", "5");
    }
}

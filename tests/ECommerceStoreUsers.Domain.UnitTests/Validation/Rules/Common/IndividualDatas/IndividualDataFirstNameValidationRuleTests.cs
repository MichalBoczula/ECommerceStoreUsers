using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.IndividualDatas;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Common.IndividualDatas
{
    public class IndividualDataFirstNameValidationRuleTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("12345")]
        public async Task IsValid_FirstNameIsInvalid_ShouldReturnError(string firstName)
        {
            // Arrange
            var rule = new IndividualDataFirstNameValidationRule();
            var validationResult = new ValidationResult();
            var individualData = CreateIndividualData(firstName: firstName);

            // Act
            await rule.IsValid(individualData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("First Name cannot be empty or white space and must contain at least one letter.");
        }

        [Fact]
        public async Task IsValid_FirstNameContainsLetter_ShouldReturnNoErrors()
        {
            // Arrange
            var rule = new IndividualDataFirstNameValidationRule();
            var validationResult = new ValidationResult();
            var individualData = CreateIndividualData(firstName: "John2");

            // Act
            await rule.IsValid(individualData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        private static IndividualData CreateIndividualData(string firstName = "John") =>
            new(firstName, "Doe", "john.doe@example.com", "1234567", CreateAddress(), CreateAddress());

        private static Address CreateAddress() => new("00-001", "Warsaw", "Main St", "10", "5");
    }
}

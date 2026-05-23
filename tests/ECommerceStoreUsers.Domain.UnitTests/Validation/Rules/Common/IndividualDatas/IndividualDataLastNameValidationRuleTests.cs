using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Entities.IndividualDatas;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Common.IndividualDatas
{
    public class IndividualDataLastNameValidationRuleTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("12345")]
        public async Task IsValid_LastNameIsInvalid_ShouldReturnError(string lastName)
        {
            // Arrange
            var rule = new IndividualDataLastNameValidationRule();
            var validationResult = new ValidationResult();
            var individualData = CreateIndividualData(lastName: lastName);

            // Act
            await rule.IsValid(individualData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Last Name cannot be empty or white space and must contain at least one letter.");
        }

        [Fact]
        public async Task IsValid_LastNameContainsLetter_ShouldReturnNoErrors()
        {
            // Arrange
            var rule = new IndividualDataLastNameValidationRule();
            var validationResult = new ValidationResult();
            var individualData = CreateIndividualData(lastName: "Doe2");

            // Act
            await rule.IsValid(individualData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        private static IndividualData CreateIndividualData(string lastName = "Doe") =>
            new("John", lastName, "john.doe@example.com", "1234567", CreateAddress(), CreateAddress());

        private static Address CreateAddress() => new("00-001", "Warsaw", "Main St", "10", "5");
    }
}

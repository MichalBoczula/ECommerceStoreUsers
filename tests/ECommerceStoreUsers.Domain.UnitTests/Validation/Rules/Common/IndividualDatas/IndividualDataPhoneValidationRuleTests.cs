using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.IndividualDatas;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Common.IndividualDatas
{
    public class IndividualDataPhoneValidationRuleTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("123456")]
        [InlineData("12345678901")]
        [InlineData("12345ab")]
        [InlineData("123 4567")]
        public async Task IsValid_PhoneIsInvalid_ShouldReturnError(string phone)
        {
            // Arrange
            var rule = new IndividualDataPhoneValidationRule();
            var validationResult = new ValidationResult();
            var individualData = CreateIndividualData(phone: phone);

            // Act
            await rule.IsValid(individualData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Phone must contain only digits and be between 7 and 10 characters long.");
        }

        [Theory]
        [InlineData("1234567")]
        [InlineData("1234567890")]
        public async Task IsValid_PhoneIsValid_ShouldReturnNoErrors(string phone)
        {
            // Arrange
            var rule = new IndividualDataPhoneValidationRule();
            var validationResult = new ValidationResult();
            var individualData = CreateIndividualData(phone: phone);

            // Act
            await rule.IsValid(individualData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        private static IndividualData CreateIndividualData(string phone = "1234567") =>
            new("John", "Doe", "john.doe@example.com", phone, CreateAddress(), CreateAddress());

        private static Address CreateAddress() => new("00-001", "Warsaw", "Main St", "10", "5");
    }
}

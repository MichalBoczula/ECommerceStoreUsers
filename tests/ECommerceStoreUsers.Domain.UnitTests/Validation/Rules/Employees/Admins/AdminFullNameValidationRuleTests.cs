using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Employees.Admins;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Employees.Admins
{
    public class AdminFullNameValidationRuleTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("John@Doe")]
        [InlineData("John#Doe")]
        [InlineData("John$Doe")]
        public async Task IsValid_FullNameContainsSpecialChars_ShouldReturnError(string invalidName)
        {
            // Arrange
            var rule = new AdminFullNameValidationRule();
            var validationResult = new ValidationResult();
            var admin = new Admin("EXT-001", invalidName, "john@example.com");

            // Act
            await rule.IsValid(admin, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Full Name cannot be null, white space, or contain special characters (@#$%^&*).");
        }
    }
}
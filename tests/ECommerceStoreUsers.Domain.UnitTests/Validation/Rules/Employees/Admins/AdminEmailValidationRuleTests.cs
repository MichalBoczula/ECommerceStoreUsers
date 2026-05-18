using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Employees.Admins;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Employees.Admins
{
    public class AdminEmailValidationRuleTests
    {
        [Theory]
        [InlineData("invalid-email")]
        [InlineData("test@domain")]
        [InlineData("@missing-user.com")]
        public async Task IsValid_EmailFormatIsInvalid_ShouldReturnError(string invalidEmail)
        {
            // Arrange
            var rule = new AdminEmailValidationRule();
            var validationResult = new ValidationResult();
            var admin = new Admin("EXT-001", "John Doe", invalidEmail);

            // Act
            await rule.IsValid(admin, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Email must be a valid format (address@domain.something).");
        }
    }
}
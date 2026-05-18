using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Employees.Admins;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Employees.Admins
{
    public class AdminExternalIdValidationRuleTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task IsValid_ExternalIdIsInvalid_ShouldReturnError(string invalidExternalId)
        {
            // Arrange
            var rule = new AdminExternalIdValidationRule();
            var validationResult = new ValidationResult();
            var admin = new Admin(invalidExternalId, "John Doe", "john@example.com");

            // Act
            await rule.IsValid(admin, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("ExternalId cannot be null or white space.");
        }

        [Fact]
        public void Describe_ShouldReturnRuleDescriptors()
        {
            // Arrange
            var rule = new AdminExternalIdValidationRule();

            // Act
            var descriptors = rule.Describe();

            // Assert
            descriptors.Count.ShouldBe(1);
            descriptors.ShouldContain(d => d.Message == "ExternalId cannot be null or white space.");
        }
    }
}
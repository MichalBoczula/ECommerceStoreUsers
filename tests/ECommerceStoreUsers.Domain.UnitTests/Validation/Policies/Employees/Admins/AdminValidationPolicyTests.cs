using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Employees.Admins;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Employees.Admins
{
    public class AdminValidationPolicyTests
    {
        [Fact]
        public async Task Validate_AdminWithInvalidData_ShouldReturnMultipleErrors()
        {
            // Arrange
            var policy = new AdminValidationPolicy();
            var admin = new Admin("", "John#Doe", "wrong-email");

            // Act
            var result = await policy.Validate(admin);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.GetValidationErrors().Count.ShouldBe(3);
            result.GetValidationErrors().ShouldContain(e => e.Name == "AdminExternalIdValidationRule");
            result.GetValidationErrors().ShouldContain(e => e.Name == "AdminFullNameValidationRule");
            result.GetValidationErrors().ShouldContain(e => e.Name == "AdminEmailValidationRule");
        }

        [Fact]
        public async Task Validate_AdminIsValid_ShouldReturnNoErrors()
        {
            // Arrange
            var policy = new AdminValidationPolicy();
            var admin = new Admin("EXT-123", "John Doe", "john.doe@company.com");

            // Act
            var result = await policy.Validate(admin);

            // Assert
            result.IsValid.ShouldBeTrue();
            result.GetValidationErrors().Count.ShouldBe(0);
        }
    }
}
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers.Entities.IndividualDatas;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Customers.Entities.IndividualDatas
{
    public class IndividualDataValidationPolicyTests
    {
        [Fact]
        public async Task Validate_IndividualDataWithInvalidData_ShouldReturnMultipleErrors()
        {
            // Arrange
            var policy = new IndividualDataValidationPolicy();
            var individualData = new IndividualData("", "123", "invalid-email", "123-456", CreateAddress(), CreateAddress());

            // Act
            var result = await policy.Validate(individualData);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.GetValidationErrors().Count.ShouldBe(4);
            result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataFirstNameValidationRule");
            result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataLastNameValidationRule");
            result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataEmailValidationRule");
            result.GetValidationErrors().ShouldContain(e => e.Name == "IndividualDataPhoneValidationRule");
        }

        [Fact]
        public async Task Validate_IndividualDataIsValid_ShouldReturnNoErrors()
        {
            // Arrange
            var policy = new IndividualDataValidationPolicy();
            var individualData = new IndividualData("John", "Doe", "john.doe@example.com", "1234567", CreateAddress(), CreateAddress());

            // Act
            var result = await policy.Validate(individualData);

            // Assert
            result.IsValid.ShouldBeTrue();
            result.GetValidationErrors().Count.ShouldBe(0);
        }

        private static Address CreateAddress() => new("00-001", "Warsaw", "Main St", "10", "5");
    }
}

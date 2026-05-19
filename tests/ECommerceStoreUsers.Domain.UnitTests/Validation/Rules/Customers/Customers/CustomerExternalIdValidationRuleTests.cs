using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Customers;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Customers.Customers
{
    public class CustomerExternalIdValidationRuleTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task IsValid_ExternalIdIsInvalid_ShouldReturnError(string invalidExternalId)
        {
            // Arrange
            var rule = new CustomerExternalIdValidationRule();
            var validationResult = new ValidationResult();
            var customer = CreateCustomer(invalidExternalId);

            // Act
            await rule.IsValid(customer, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("ExternalId cannot be null or white space.");
        }

        [Fact]
        public void Describe_ShouldReturnRuleDescriptors()
        {
            // Arrange
            var rule = new CustomerExternalIdValidationRule();

            // Act
            var descriptors = rule.Describe();

            // Assert
            descriptors.Count.ShouldBe(1);
            descriptors.ShouldContain(d => d.Message == "ExternalId cannot be null or white space.");
        }

        private static Customer CreateCustomer(string externalId)
        {
            var address = new Address("00-001", "Warsaw", "Main", "10", "2");
            var individualData = new IndividualData("John", "Doe", "john@example.com", "123456789", address, address);
            return new Customer(externalId, individualData);
        }
    }
}

using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Customers;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Customers.Customers
{
    public class CustomerEmptyGuidValidationRuleTests
    {
        [Fact]
        public async Task IsValid_ExternalIdIsEmptyGuid_ShouldReturnError()
        {
            // Arrange
            var rule = new CustomerEmptyGuidValidationRule();
            var validationResult = new ValidationResult();
            var customer = CreateCustomer(Guid.Empty.ToString());

            // Act
            await rule.IsValid(customer, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("ExternalId cannot be an empty GUID.");
        }

        [Fact]
        public async Task IsValid_ClientIdIsEmptyGuid_ShouldReturnError()
        {
            // Arrange
            var rule = new CustomerEmptyGuidValidationRule();
            var validationResult = new ValidationResult();
            var customer = CreateRehydratedCustomer(Guid.Empty, "EXT-123");

            // Act
            await rule.IsValid(customer, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("ClientId cannot be an empty GUID.");
        }

        [Fact]
        public async Task IsValid_ExternalIdIsValidGuid_ShouldReturnNoError()
        {
            // Arrange
            var rule = new CustomerEmptyGuidValidationRule();
            var validationResult = new ValidationResult();
            var customer = CreateCustomer(Guid.NewGuid().ToString());

            // Act
            await rule.IsValid(customer, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        [Fact]
        public async Task IsValid_ExternalIdIsNull_ShouldReturnNoError()
        {
            // Arrange
            var rule = new CustomerEmptyGuidValidationRule();
            var validationResult = new ValidationResult();
            var customer = CreateRehydratedCustomer(Guid.NewGuid(), null!);

            // Act
            await rule.IsValid(customer, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        [Fact]
        public void Describe_ShouldReturnRuleDescriptors()
        {
            // Arrange
            var rule = new CustomerEmptyGuidValidationRule();

            // Act
            var descriptors = rule.Describe();

            // Assert
            descriptors.Count.ShouldBe(2);
            descriptors.ShouldContain(d => d.Message == "ClientId cannot be an empty GUID.");
            descriptors.ShouldContain(d => d.Message == "ExternalId cannot be an empty GUID.");
        }

        private static Customer CreateCustomer(string externalId)
        {
            var address = new Address("00-001", "Warsaw", "Main", "10", "2");
            var individualData = new IndividualData("John", "Doe", "john@example.com", "123456789", address, address);
            return new Customer(externalId, individualData);
        }

        private static Customer CreateRehydratedCustomer(Guid id, string externalId)
        {
            var address = new Address("00-001", "Warsaw", "Main", "10", "2");
            var individualData = new IndividualData("John", "Doe", "john@example.com", "123456789", address, address);
            return Customer.Rehydrate(id, externalId, individualData, new List<CompanyData>());
        }
    }
}

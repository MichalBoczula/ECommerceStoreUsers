using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Customers;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Customers.Customers
{
    public class CustomerCompanyTaxIdValidationRuleTests
    {
        [Fact]
        public async Task IsValid_CustomerHasCompanyWithSameTaxId_ShouldReturnError()
        {
            // Arrange
            var rule = new CustomerCompanyTaxIdValidationRule();
            var validationResult = new ValidationResult();
            var customer = CreateCustomer();
            customer.AddCompany("Acme Sp. z o.o.", "1234567890", CreateAddress(), CreateAddress());
            customer.AddCompany("Contoso Sp. z o.o.", "1234567890", CreateAddress(), CreateAddress());

            // Act
            await rule.IsValid(customer, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Customer already contains a company with the same Tax Id.");
        }

        [Fact]
        public async Task IsValid_CustomerHasDuplicatedTaxIdBeforeLastCompany_ShouldReturnError()
        {
            // Arrange
            var rule = new CustomerCompanyTaxIdValidationRule();
            var validationResult = new ValidationResult();
            var customer = CreateCustomer();
            customer.AddCompany("Acme Sp. z o.o.", "1234567890", CreateAddress(), CreateAddress());
            customer.AddCompany("Contoso Sp. z o.o.", "1234567890", CreateAddress(), CreateAddress());
            customer.AddCompany("Fabrikam Sp. z o.o.", "0987654321", CreateAddress(), CreateAddress());

            // Act
            await rule.IsValid(customer, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Customer already contains a company with the same Tax Id.");
        }

        [Fact]
        public async Task IsValid_CustomerHasCompanyWithDifferentTaxId_ShouldReturnNoErrors()
        {
            // Arrange
            var rule = new CustomerCompanyTaxIdValidationRule();
            var validationResult = new ValidationResult();
            var customer = CreateCustomer();
            customer.AddCompany("Acme Sp. z o.o.", "1234567890", CreateAddress(), CreateAddress());
            customer.AddCompany("Contoso Sp. z o.o.", "0987654321", CreateAddress(), CreateAddress());

            // Act
            await rule.IsValid(customer, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        [Fact]
        public async Task IsValid_CustomerHasNoCompanies_ShouldReturnNoErrors()
        {
            // Arrange
            var rule = new CustomerCompanyTaxIdValidationRule();
            var validationResult = new ValidationResult();
            var customer = CreateCustomer();

            // Act
            await rule.IsValid(customer, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        [Fact]
        public void Describe_ShouldReturnRuleDescriptors()
        {
            // Arrange
            var rule = new CustomerCompanyTaxIdValidationRule();

            // Act
            var descriptors = rule.Describe();

            // Assert
            descriptors.Count.ShouldBe(1);
            descriptors.ShouldContain(d => d.Message == "Customer already contains a company with the same Tax Id.");
        }

        private static Customer CreateCustomer()
        {
            var address = CreateAddress();
            var individualData = new IndividualData("John", "Doe", "john@example.com", "123456789", address, address);
            return new Customer("EXT-123", individualData);
        }

        private static Address CreateAddress() => new("00-001", "Warsaw", "Main", "10", "2");
    }
}

using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Customers.Customers
{
    public class CustomerValidationPolicyTests
    {
        [Fact]
        public async Task Validate_CustomerWithInvalidExternalId_ShouldReturnError()
        {
            // Arrange
            var policy = new CustomerValidationPolicy();
            var customer = CreateCustomer("");

            // Act
            var result = await policy.Validate(customer);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.GetValidationErrors().Count.ShouldBe(1);
            result.GetValidationErrors().ShouldContain(e => e.Name == "CustomerExternalIdValidationRule");
        }

        [Fact]
        public async Task Validate_CustomerIsValid_ShouldReturnNoErrors()
        {
            // Arrange
            var policy = new CustomerValidationPolicy();
            var customer = CreateCustomer("EXT-123");

            // Act
            var result = await policy.Validate(customer);

            // Assert
            result.IsValid.ShouldBeTrue();
            result.GetValidationErrors().Count.ShouldBe(0);
        }


        [Fact]
        public async Task Validate_CustomerHasCompaniesWithSameTaxId_ShouldReturnError()
        {
            // Arrange
            var policy = new CustomerValidationPolicy();
            var customer = CreateCustomer("EXT-123");
            var address = new Address("00-001", "Warsaw", "Main", "10", "2");
            customer.AddCompany("Acme Sp. z o.o.", "1234567890", address, address);
            customer.AddCompany("Contoso Sp. z o.o.", "1234567890", address, address);

            // Act
            var result = await policy.Validate(customer);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.GetValidationErrors().Count.ShouldBe(1);
            result.GetValidationErrors().ShouldContain(e => e.Name == "CompanyDataTaxIdUniquenessValidationRule");
        }

        private static Customer CreateCustomer(string externalId)
        {
            var address = new Address("00-001", "Warsaw", "Main", "10", "2");
            var individualData = new IndividualData("John", "Doe", "john@example.com", "123456789", address, address);
            return new Customer(externalId, individualData);
        }
    }
}

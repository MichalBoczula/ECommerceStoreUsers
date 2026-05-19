using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Entities.CompanyDatas;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Customers.Entities.CompanyDatas
{
    public class CompanyDataCompanyNameValidationRuleTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task IsValid_CompanyNameIsInvalid_ShouldReturnError(string companyName)
        {
            // Arrange
            var rule = new CompanyDataCompanyNameValidationRule();
            var validationResult = new ValidationResult();
            var companyData = CreateCompanyData(companyName: companyName);

            // Act
            await rule.IsValid(companyData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Company Name cannot be empty or white space.");
        }

        [Theory]
        [InlineData("Acme Sp. z o.o.")]
        [InlineData("ACME")]
        public async Task IsValid_CompanyNameIsValid_ShouldReturnNoErrors(string companyName)
        {
            // Arrange
            var rule = new CompanyDataCompanyNameValidationRule();
            var validationResult = new ValidationResult();
            var companyData = CreateCompanyData(companyName: companyName);

            // Act
            await rule.IsValid(companyData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        private static CompanyData CreateCompanyData(string companyName = "Acme Sp. z o.o.") =>
            new("1234567890", companyName, CreateAddress(), CreateAddress());

        private static Address CreateAddress() => new("00-001", "Warsaw", "Main St", "10", "5");
    }
}

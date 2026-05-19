using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Entities.CompanyDatas;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Customers.Entities.CompanyDatas
{
    public class CompanyDataTaxIdValidationRuleTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("123456789")]
        [InlineData("12345678901")]
        [InlineData("123456789A")]
        [InlineData("123-456-78-90")]
        [InlineData("123 456 7890")]
        public async Task IsValid_TaxIdIsInvalid_ShouldReturnError(string taxId)
        {
            // Arrange
            var rule = new CompanyDataTaxIdValidationRule();
            var validationResult = new ValidationResult();
            var companyData = CreateCompanyData(taxId: taxId);

            // Act
            await rule.IsValid(companyData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Tax Id must be a valid Polish NIP containing exactly 10 digits.");
        }

        [Theory]
        [InlineData("1234567890")]
        [InlineData("0000000000")]
        public async Task IsValid_TaxIdIsValid_ShouldReturnNoErrors(string taxId)
        {
            // Arrange
            var rule = new CompanyDataTaxIdValidationRule();
            var validationResult = new ValidationResult();
            var companyData = CreateCompanyData(taxId: taxId);

            // Act
            await rule.IsValid(companyData, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        private static CompanyData CreateCompanyData(string taxId = "1234567890") =>
            new(taxId, "Acme Sp. z o.o.", CreateAddress(), CreateAddress());

        private static Address CreateAddress() => new("00-001", "Warsaw", "Main St", "10", "5");
    }
}

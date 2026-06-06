using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.CompanyDatas;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Common.CompanyDatas
{
    public class CompanyDataTaxIdUniquenessValidationRuleTests
    {
        [Fact]
        public async Task IsValid_CompaniesContainDuplicatedTaxId_ShouldReturnError()
        {
            // Arrange
            var rule = new CompanyDataTaxIdUniquenessValidationRule();
            var validationResult = new ValidationResult();
            var companies = new List<CompanyData>
            {
                CreateCompanyData("1234567890"),
                CreateCompanyData("1234567890")
            };

            // Act
            await rule.IsValid(companies, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("Customer already contains a company with the same Tax Id.");
            validationResult.GetValidationErrors().First().Name.ShouldBe(nameof(CompanyDataTaxIdUniquenessValidationRule));
            validationResult.GetValidationErrors().First().Entity.ShouldBe(nameof(CompanyData));
        }

        [Fact]
        public async Task IsValid_CompaniesContainUniqueTaxIds_ShouldReturnNoErrors()
        {
            // Arrange
            var rule = new CompanyDataTaxIdUniquenessValidationRule();
            var validationResult = new ValidationResult();
            var companies = new List<CompanyData>
            {
                CreateCompanyData("1234567890"),
                CreateCompanyData("0987654321")
            };

            // Act
            await rule.IsValid(companies, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        [Fact]
        public async Task IsValid_CompaniesListIsEmpty_ShouldReturnNoErrors()
        {
            // Arrange
            var rule = new CompanyDataTaxIdUniquenessValidationRule();
            var validationResult = new ValidationResult();
            var companies = new List<CompanyData>();

            // Act
            await rule.IsValid(companies, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(0);
        }

        [Fact]
        public void Describe_ShouldReturnRuleDescriptors()
        {
            // Arrange
            var rule = new CompanyDataTaxIdUniquenessValidationRule();

            // Act
            var descriptors = rule.Describe();

            // Assert
            descriptors.Count.ShouldBe(1);
            descriptors.ShouldContain(d => d.Message == "Customer already contains a company with the same Tax Id.");
            descriptors.ShouldContain(d => d.Name == nameof(CompanyDataTaxIdUniquenessValidationRule));
            descriptors.ShouldContain(d => d.Entity == nameof(CompanyData));
        }

        private static CompanyData CreateCompanyData(string taxId) =>
            new(taxId, "Acme Sp. z o.o.", CreateAddress(), CreateAddress());

        private static Address CreateAddress() => new("00-001", "Warsaw", "Main St", "10", "5");
    }
}

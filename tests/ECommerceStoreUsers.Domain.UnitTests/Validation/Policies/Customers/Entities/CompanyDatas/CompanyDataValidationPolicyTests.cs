using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Customers.Entities.CompanyDatas;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Customers.Entities.CompanyDatas
{
    public class CompanyDataValidationPolicyTests
    {
        [Fact]
        public async Task Validate_CompanyDataWithInvalidData_ShouldReturnMultipleErrors()
        {
            // Arrange
            var policy = new CompanyDataValidationPolicy();
            var companyData = new CompanyData(Guid.NewGuid(), "", "123456789A", CreateAddress(), CreateAddress());

            // Act
            var result = await policy.Validate(companyData);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.GetValidationErrors().Count.ShouldBe(2);
            result.GetValidationErrors().ShouldContain(e => e.Name == "CompanyDataCompanyNameValidationRule");
            result.GetValidationErrors().ShouldContain(e => e.Name == "CompanyDataTaxIdValidationRule");
        }

        [Fact]
        public async Task Validate_CompanyDataIsValid_ShouldReturnNoErrors()
        {
            // Arrange
            var policy = new CompanyDataValidationPolicy();
            var companyData = new CompanyData(Guid.NewGuid(), "Acme Sp. z o.o.", "1234567890", CreateAddress(), CreateAddress());

            // Act
            var result = await policy.Validate(companyData);

            // Assert
            result.IsValid.ShouldBeTrue();
            result.GetValidationErrors().Count.ShouldBe(0);
        }

        private static Address CreateAddress() => new("00-001", "Warsaw", "Main St", "10", "5");
    }
}

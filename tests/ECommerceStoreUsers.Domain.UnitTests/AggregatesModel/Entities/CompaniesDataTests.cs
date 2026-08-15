using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.AggregatesModel.Entities
{
    public class CompaniesDataTests
    {
        [Fact]
        public void Constructor_ShouldInitializeAllProperties()
        {
            // Arrange
            var billing = CreateAddress(city: "Warsaw");
            var shipping = CreateAddress(city: "Krakow");

            // Act
            var company = new CompanyData("1234567890", "Contoso", billing, shipping);

            // Assert
            company.Id.ShouldNotBe(Guid.Empty);
            company.TaxId.ShouldBe("1234567890");
            company.CompanyName.ShouldBe("Contoso");
            company.BillingAddress.ShouldBe(billing);
            company.ShippingAddress.ShouldBe(shipping);
        }

        [Fact]
        public void UpdateCompanyDetails_ShouldUpdateAllProperties()
        {
            // Arrange
            var company = new CompanyData("1234567890", "Contoso", CreateAddress(), CreateAddress());
            var newBilling = CreateAddress(city: "Gdansk");
            var newShipping = CreateAddress(city: "Lodz");

            // Act
            company.UpdateCompanyDetails("9999999999", "Fabrikam", newBilling, newShipping);

            // Assert
            company.TaxId.ShouldBe("9999999999");
            company.CompanyName.ShouldBe("Fabrikam");
            company.BillingAddress.ShouldBe(newBilling);
            company.ShippingAddress.ShouldBe(newShipping);
        }

        [Fact]
        public void Rehydrate_ShouldRebuildCompanyWithGivenValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var billing = CreateAddress(city: "Warsaw");
            var shipping = CreateAddress(city: "Krakow");

            // Act
            var company = CompanyData.Rehydrate(id, "1234567890", "Contoso", billing, shipping);

            // Assert
            company.Id.ShouldBe(id);
            company.TaxId.ShouldBe("1234567890");
            company.CompanyName.ShouldBe("Contoso");
            company.BillingAddress.ShouldBe(billing);
            company.ShippingAddress.ShouldBe(shipping);
        }

        private static Address CreateAddress(
            string postalCode = "00-001",
            string city = "Warsaw",
            string street = "Main St",
            string building = "10",
            string apartment = "5") =>
            new(postalCode, city, street, building, apartment);
    }
}

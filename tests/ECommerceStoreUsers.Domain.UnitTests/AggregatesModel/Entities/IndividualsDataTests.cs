using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.AggregatesModel.Entities
{
    public class IndividualsDataTests
    {
        [Fact]
        public void Constructor_ShouldInitializeAllProperties()
        {
            // Arrange
            var billing = CreateAddress(city: "Warsaw");
            var shipping = CreateAddress(city: "Krakow");

            // Act
            var individual = new IndividualData("John", "Doe", "john.doe@example.com", "1234567", billing, shipping);

            // Assert
            individual.FirstName.ShouldBe("John");
            individual.LastName.ShouldBe("Doe");
            individual.Email.ShouldBe("john.doe@example.com");
            individual.Phone.ShouldBe("1234567");
            individual.BillingAddress.ShouldBe(billing);
            individual.ShippingAddress.ShouldBe(shipping);
        }

        [Fact]
        public void UpdateIndividualData_ShouldUpdateAllProperties()
        {
            // Arrange
            var individual = new IndividualData("John", "Doe", "john.doe@example.com", "1234567", CreateAddress(), CreateAddress());
            var newBilling = CreateAddress(city: "Gdansk");
            var newShipping = CreateAddress(city: "Lodz");

            // Act
            individual.UpdateIndividualData("Jane", "Smith", "jane.smith@example.com", "7654321", newBilling, newShipping);

            // Assert
            individual.FirstName.ShouldBe("Jane");
            individual.LastName.ShouldBe("Smith");
            individual.Email.ShouldBe("jane.smith@example.com");
            individual.Phone.ShouldBe("7654321");
            individual.BillingAddress.ShouldBe(newBilling);
            individual.ShippingAddress.ShouldBe(newShipping);
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

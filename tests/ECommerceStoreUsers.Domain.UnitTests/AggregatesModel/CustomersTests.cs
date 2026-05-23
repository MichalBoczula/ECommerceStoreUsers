using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.AggregatesModel
{
    public class CustomersTests
    {
        [Fact]
        public void Constructor_ShouldInitializeCoreProperties()
        {
            // Arrange
            var individual = CreateIndividualData();

            // Act
            var customer = new Customer("ext-123", individual);

            // Assert
            customer.Id.ShouldNotBe(Guid.Empty);
            customer.ExternalId.ShouldBe("ext-123");
            customer.Individual.ShouldBe(individual);
            customer.Companies.Count.ShouldBe(0);
            customer.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void UpdateIndividualData_ShouldReplaceIndividualAndUpdateTimestamp()
        {
            // Arrange
            var customer = new Customer("ext-123", CreateIndividualData(firstName: "John"));
            var originalUpdatedAt = customer.UpdatedAt;
            var newData = CreateIndividualData(firstName: "Jane");

            // Act
            customer.UpdateIndividualData(newData);

            // Assert
            customer.Individual.ShouldBe(newData);
            customer.Individual.FirstName.ShouldBe("Jane");
            customer.UpdatedAt.ShouldBeGreaterThanOrEqualTo(originalUpdatedAt);
        }

        [Fact]
        public void AddCompany_ShouldAddCompanyAndUpdateTimestamp()
        {
            // Arrange
            var customer = new Customer("ext-123", CreateIndividualData());
            var billing = CreateAddress(city: "Warsaw");
            var shipping = CreateAddress(city: "Krakow");
            var originalUpdatedAt = customer.UpdatedAt;

            // Act
            customer.AddCompany("Contoso", "1234567890", billing, shipping);

            // Assert
            customer.Companies.Count.ShouldBe(1);
            var company = customer.Companies.Single();
            company.CompanyName.ShouldBe("Contoso");
            company.TaxId.ShouldBe("1234567890");
            company.BillingAddress.ShouldBe(billing);
            company.ShippingAddress.ShouldBe(shipping);
            customer.UpdatedAt.ShouldBeGreaterThanOrEqualTo(originalUpdatedAt);
        }

        [Fact]
        public void Rehydrate_ShouldRebuildCustomerWithGivenIdAndCompanies()
        {
            // Arrange
            var id = Guid.NewGuid();
            var individual = CreateIndividualData();
            var companies = new List<CompanyData>
            {
                new("1234567890", "Contoso", CreateAddress(), CreateAddress()),
                new("9876543210", "Fabrikam", CreateAddress(city: "Wroclaw"), CreateAddress(city: "Poznan"))
            };

            // Act
            var customer = Customer.Rehydrate(id, "ext-999", individual, companies);

            // Assert
            customer.Id.ShouldBe(id);
            customer.ExternalId.ShouldBe("ext-999");
            customer.Individual.ShouldBe(individual);
            customer.Companies.Count.ShouldBe(2);
            customer.Companies.Select(c => c.CompanyName).ShouldBe(new[] { "Contoso", "Fabrikam" });
        }

        private static IndividualData CreateIndividualData(string firstName = "John") =>
            new(firstName, "Doe", "john.doe@example.com", "1234567", CreateAddress(), CreateAddress());

        private static Address CreateAddress(
            string postalCode = "00-001",
            string city = "Warsaw",
            string street = "Main St",
            string building = "10",
            string apartment = "5") =>
            new(postalCode, city, street, building, apartment);
    }
}

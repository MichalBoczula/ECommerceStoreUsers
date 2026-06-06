using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using Shouldly;

namespace ECommerceStoreUsers.Application.UnitTests.Mapping
{
    public class MappingConfigTests
    {
        [Fact]
        public void MapToDomain_CreateCustomerRequestDto_ShouldMapCustomerWithIndividual()
        {
            // Arrange
            var request = new CreateCustomerRequestDto
            {
                ExternalId = "external-1",
                Individual = CreateIndividualRequestDto()
            };

            // Act
            var result = MappingConfig.MapToDomain(request);

            // Assert
            result.ExternalId.ShouldBe("external-1");
            result.Individual.FirstName.ShouldBe("Jan");
            result.Individual.LastName.ShouldBe("Kowalski");
            result.Individual.Email.ShouldBe("jan.kowalski@example.com");
            result.Individual.Phone.ShouldBe("123456789");
            result.Individual.BillingAddress.PostalCode.ShouldBe("00-100");
            result.Individual.BillingAddress.City.ShouldBe("Warsaw");
            result.Individual.BillingAddress.Street.ShouldBe("Main");
            result.Individual.BillingAddress.BuildingNumber.ShouldBe("1");
            result.Individual.BillingAddress.ApartmentNumber.ShouldBe("10");
            result.Individual.ShippingAddress.PostalCode.ShouldBe("00-200");
            result.Individual.ShippingAddress.City.ShouldBe("Krakow");
        }

        [Fact]
        public void MapToDomain_CreateCustomerRequestDto_ShouldMapCustomerWithCompanyData()
        {
            // Arrange
            var request = new CreateCustomerRequestDto
            {
                ExternalId = "external-company-1",
                Individual = CreateIndividualRequestDto(),
                Companies =
                [
                    new AddCompanyRequestDto
                    {
                        TaxId = "1234567890",
                        CompanyName = "Example Company",
                        BillingAddress = new AddressRequestDto
                        {
                            PostalCode = "30-300",
                            City = "Poznan",
                            Street = "Business",
                            BuildingNumber = "3",
                            ApartmentNumber = "15"
                        },
                        ShippingAddress = new AddressRequestDto
                        {
                            PostalCode = "40-400",
                            City = "Lodz",
                            Street = "Industry",
                            BuildingNumber = "4",
                            ApartmentNumber = "16"
                        }
                    }
                ]
            };

            // Act
            var result = MappingConfig.MapToDomain(request);

            // Assert
            var company = result.Companies.ShouldHaveSingleItem();
            company.TaxId.ShouldBe("1234567890");
            company.CompanyName.ShouldBe("Example Company");
            company.BillingAddress.PostalCode.ShouldBe("30-300");
            company.BillingAddress.City.ShouldBe("Poznan");
            company.ShippingAddress.PostalCode.ShouldBe("40-400");
            company.ShippingAddress.City.ShouldBe("Lodz");
        }

        [Fact]
        public void MapToDomain_IndividualDataRequestDto_ShouldMapAllFields()
        {
            // Arrange
            var request = CreateIndividualRequestDto();

            // Act
            var result = MappingConfig.MapToDomain(request);

            // Assert
            result.FirstName.ShouldBe(request.FirstName);
            result.LastName.ShouldBe(request.LastName);
            result.Email.ShouldBe(request.Email);
            result.Phone.ShouldBe(request.Phone);
            result.BillingAddress.PostalCode.ShouldBe(request.BillingAddress.PostalCode);
            result.BillingAddress.City.ShouldBe(request.BillingAddress.City);
            result.BillingAddress.Street.ShouldBe(request.BillingAddress.Street);
            result.BillingAddress.BuildingNumber.ShouldBe(request.BillingAddress.BuildingNumber);
            result.BillingAddress.ApartmentNumber.ShouldBe(request.BillingAddress.ApartmentNumber);
            result.ShippingAddress.PostalCode.ShouldBe(request.ShippingAddress.PostalCode);
            result.ShippingAddress.City.ShouldBe(request.ShippingAddress.City);
            result.ShippingAddress.Street.ShouldBe(request.ShippingAddress.Street);
            result.ShippingAddress.BuildingNumber.ShouldBe(request.ShippingAddress.BuildingNumber);
            result.ShippingAddress.ApartmentNumber.ShouldBe(request.ShippingAddress.ApartmentNumber);
        }

        [Fact]
        public void MapAddress_ShouldMapAllFields()
        {
            // Arrange
            var request = new AddressRequestDto
            {
                PostalCode = "11-111",
                City = "Gdansk",
                Street = "Dluga",
                BuildingNumber = "22",
                ApartmentNumber = null
            };

            // Act
            var result = MappingConfig.MapAddress(request);

            // Assert
            result.PostalCode.ShouldBe(request.PostalCode);
            result.City.ShouldBe(request.City);
            result.Street.ShouldBe(request.Street);
            result.BuildingNumber.ShouldBe(request.BuildingNumber);
            result.ApartmentNumber.ShouldBeNull();
        }

        [Fact]
        public void MapToResponse_Customer_ShouldMapNestedObjectsAndCompanies()
        {
            // Arrange
            var individual = new IndividualData(
                "Anna",
                "Nowak",
                "anna.nowak@example.com",
                "987654321",
                new Address("33-333", "Lodz", "Piotrkowska", "7", "8"),
                new Address("44-444", "Poznan", "Polna", "9", null));

            var customer = new Customer("ext-2", individual);
            customer.AddCompany(
                "Contoso",
                "1234567890",
                new Address("55-555", "Wroclaw", "Legnicka", "2", "3"),
                new Address("66-666", "Szczecin", "Morska", "4", null));

            // Act
            var result = MappingConfig.MapToResponse(customer);

            // Assert
            result.Id.ShouldBe(customer.Id);
            result.ExternalId.ShouldBe(customer.ExternalId);
            result.UpdatedAt.ShouldBe(customer.UpdatedAt);
            result.Individual.FirstName.ShouldBe("Anna");
            result.Individual.LastName.ShouldBe("Nowak");
            result.Individual.Email.ShouldBe("anna.nowak@example.com");
            result.Companies.Count.ShouldBe(1);

            var company = result.Companies.Single();
            company.CompanyName.ShouldBe("Contoso");
            company.TaxId.ShouldBe("1234567890");
            company.BillingAddress.PostalCode.ShouldBe("55-555");
            company.ShippingAddress.PostalCode.ShouldBe("66-666");
        }

        [Fact]
        public void MapToResponse_IndividualData_ShouldMapAllFields()
        {
            // Arrange
            var individual = new IndividualData(
                "Tom",
                "Smith",
                "tom.smith@example.com",
                "555333111",
                new Address("70-700", "Bydgoszcz", "Leśna", "12", "1"),
                new Address("80-800", "Torun", "Parkowa", "13", null));

            // Act
            var result = MappingConfig.MapToResponse(individual);

            // Assert
            result.FirstName.ShouldBe(individual.FirstName);
            result.LastName.ShouldBe(individual.LastName);
            result.Email.ShouldBe(individual.Email);
            result.Phone.ShouldBe(individual.Phone);
            result.BillingAddress.PostalCode.ShouldBe(individual.BillingAddress.PostalCode);
            result.ShippingAddress.PostalCode.ShouldBe(individual.ShippingAddress.PostalCode);
        }

        [Fact]
        public void MapToResponse_CompanyData_ShouldMapAllFields()
        {
            // Arrange
            var company = new CompanyData(
                "5554443332",
                "Fabrikam",
                new Address("90-900", "Opole", "Krotka", "5", "2"),
                new Address("91-901", "Olsztyn", "Dluga", "6", null));

            // Act
            var result = MappingConfig.MapToResponse(company);

            // Assert
            result.Id.ShouldBe(company.Id);
            result.TaxId.ShouldBe(company.TaxId);
            company.CompanyName.ShouldBe(company.CompanyName);
            result.BillingAddress.City.ShouldBe(company.BillingAddress.City);
            result.ShippingAddress.City.ShouldBe(company.ShippingAddress.City);
        }

        [Fact]
        public void MapToResponse_Address_ShouldMapAllFields()
        {
            // Arrange
            var address = new Address("12-345", "Katowice", "Słoneczna", "77", "12");

            // Act
            var result = MappingConfig.MapToResponse(address);

            // Assert
            result.PostalCode.ShouldBe(address.PostalCode);
            result.City.ShouldBe(address.City);
            result.Street.ShouldBe(address.Street);
            result.BuildingNumber.ShouldBe(address.BuildingNumber);
            result.ApartmentNumber.ShouldBe(address.ApartmentNumber);
        }


        [Fact]
        public void MapToDomain_CreateAdminRequestDto_ShouldMapAdminFields()
        {
            // Arrange
            var request = new CreateAdminRequestDto
            {
                ExternalId = "entra-id|admin-777",
                FullName = "Marek Nowak",
                Email = "marek.nowak@example.com"
            };

            // Act
            var result = MappingConfig.MapToDomain(request);

            // Assert
            result.Id.ShouldNotBe(Guid.Empty);
            result.ExternalId.ShouldBe(request.ExternalId);
            result.FullName.ShouldBe(request.FullName);
            result.Email.ShouldBe(request.Email);
            result.IsActive.ShouldBeTrue();
            result.LastLoginAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        }

        [Fact]
        public void MapToResponse_Admin_ShouldMapAllFields()
        {
            // Arrange
            var admin = Admin.Rehydrate(
                id: Guid.NewGuid(),
                externalId: "entra-id|admin-123",
                fullName: "Piotr Zielinski",
                email: "piotr.zielinski@example.com",
                isActive: true,
                lastLoginAt: DateTime.UtcNow.AddDays(-2)
            );

            // Act
            var result = MappingConfig.MapToResponse(admin);

            // Assert
            result.Id.ShouldBe(admin.Id);
            result.ExternalId.ShouldBe(admin.ExternalId);
            result.FullName.ShouldBe(admin.FullName);
            result.Email.ShouldBe(admin.Email);
            result.IsActive.ShouldBe(admin.IsActive);
            result.LastLoginAt.ShouldBe(admin.LastLoginAt);
        }

        private static IndividualDataRequestDto CreateIndividualRequestDto()
        {
            return new IndividualDataRequestDto
            {
                FirstName = "Jan",
                LastName = "Kowalski",
                Email = "jan.kowalski@example.com",
                Phone = "123456789",
                BillingAddress = new AddressRequestDto
                {
                    PostalCode = "00-100",
                    City = "Warsaw",
                    Street = "Main",
                    BuildingNumber = "1",
                    ApartmentNumber = "10"
                },
                ShippingAddress = new AddressRequestDto
                {
                    PostalCode = "00-200",
                    City = "Krakow",
                    Street = "Market",
                    BuildingNumber = "2",
                    ApartmentNumber = "20"
                }
            };
        }

    }
}
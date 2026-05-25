using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Integration.Tests
{
    public sealed class CustomerRepositoryTests : IClassFixture<MongoDbTestFixture>
    {
        private readonly MongoDbTestFixture _fixture;

        public CustomerRepositoryTests(MongoDbTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreateCustomer_ShouldSaveCustomerAndEmbeddedCollections()
        {
            // arrange
            var databaseName = $"user-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<ICustomerRepository>();

            var customer = CreateTestCustomer();
            customer.AddCompany(
                name: "Test Company LLC",
                taxId: "PL1234567890",
                billing: CreateDefaultAddress("00-001"),
                shipping: CreateDefaultAddress("00-002"));

            // act
            await repository.CreateCustomer(customer, CancellationToken.None);

            // assert
            var result = await repository.GetByIdAsync(customer.Id, CancellationToken.None);

            result.ShouldNotBeNull();
            result.Id.ShouldBe(customer.Id);
            result.ExternalId.ShouldBe(customer.ExternalId);

            result.Individual.FirstName.ShouldBe(customer.Individual.FirstName);
            result.Individual.LastName.ShouldBe(customer.Individual.LastName);
            result.Individual.Email.ShouldBe(customer.Individual.Email);
            result.Individual.Phone.ShouldBe(customer.Individual.Phone);
            result.Individual.BillingAddress.City.ShouldBe("Warszawa");

            result.Companies.Count.ShouldBe(1);
            var companyResult = result.Companies.First();
            companyResult.CompanyName.ShouldBe("Test Company LLC");
            companyResult.TaxId.ShouldBe("PL1234567890");
        }

        [Fact]
        public async Task UpdateCustomer_ShouldModifyExistingDatabaseDocument()
        {
            // arrange
            var databaseName = $"user-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<ICustomerRepository>();

            var customer = CreateTestCustomer();
            await repository.CreateCustomer(customer, CancellationToken.None);

            var customerToUpdate = await repository.GetByIdAsync(customer.Id, CancellationToken.None);
            customerToUpdate.ShouldNotBeNull();

            var updatedIndividual = new IndividualData(
                "Janusz",
                "Kowalski",
                "janusz.k@test.pl",
                "+48111222333",
                CreateDefaultAddress("02-222"),
                CreateDefaultAddress("02-333"));

            customerToUpdate.UpdateIndividualData(updatedIndividual);

            // act
            await repository.UpdateCustomer(customerToUpdate, CancellationToken.None);

            // assert
            var updatedResult = await repository.GetByIdAsync(customer.Id, CancellationToken.None);
            updatedResult.ShouldNotBeNull();
            updatedResult.Individual.FirstName.ShouldBe("Janusz");
            updatedResult.Individual.Email.ShouldBe("janusz.k@test.pl");
            updatedResult.UpdatedAt.ShouldBeGreaterThan(customer.UpdatedAt);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCustomer_WhenMatchExists()
        {
            // arrange
            var databaseName = $"user-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<ICustomerRepository>();

            var customer = CreateTestCustomer();
            await repository.CreateCustomer(customer, CancellationToken.None);

            // act
            var result = await repository.GetByIdAsync(customer.Id, CancellationToken.None);

            // assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(customer.Id);
            result.ExternalId.ShouldBe(customer.ExternalId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
        {
            // arrange
            var databaseName = $"user-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<ICustomerRepository>();

            var nonExistentId = Guid.NewGuid();

            // act
            var result = await repository.GetByIdAsync(nonExistentId, CancellationToken.None);

            // assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetByExternalIdAsync_ShouldReturnCustomer_WhenMatchExists()
        {
            // arrange
            var databaseName = $"user-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<ICustomerRepository>();

            var targetExternalId = $"entra-id|{Guid.NewGuid()}";
            var customer = new Customer(targetExternalId, CreateTestIndividualData());
            await repository.CreateCustomer(customer, CancellationToken.None);

            // act
            var result = await repository.GetByExternalIdAsync(targetExternalId, CancellationToken.None);

            // assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(customer.Id);
            result.ExternalId.ShouldBe(targetExternalId);
        }

        [Fact]
        public async Task GetByExternalIdAsync_ShouldReturnNull_WhenExternalIdDoesNotExist()
        {
            // arrange
            var databaseName = $"user-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<ICustomerRepository>();

            var nonExistentExternalId = $"entra-id|non-existent-{Guid.NewGuid()}";

            // act
            var result = await repository.GetByExternalIdAsync(nonExistentExternalId, CancellationToken.None);

            // assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task UpdateCustomer_ShouldModifyEmbeddedCompanyDetails()
        {
            // arrange
            var databaseName = $"user-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<ICustomerRepository>();

            var customer = CreateTestCustomer();
            customer.AddCompany("Original Company", "TAX-111", CreateDefaultAddress("00-001"), CreateDefaultAddress("00-001"));
            await repository.CreateCustomer(customer, CancellationToken.None);

            var customerToUpdate = await repository.GetByIdAsync(customer.Id, CancellationToken.None);
            customerToUpdate.ShouldNotBeNull();

            var company = customerToUpdate.Companies.First();
            company.UpdateCompanyDetails("TAX-999", "Updated Company Name", company.BillingAddress, company.ShippingAddress);

            // act
            await repository.UpdateCustomer(customerToUpdate, CancellationToken.None);

            // assert
            var updatedResult = await repository.GetByIdAsync(customer.Id, CancellationToken.None);
            updatedResult.ShouldNotBeNull();
            updatedResult.Companies.Count.ShouldBe(1);
            updatedResult.Companies.First().CompanyName.ShouldBe("Updated Company Name");
            updatedResult.Companies.First().TaxId.ShouldBe("TAX-999");
        }

        [Fact]
        public async Task UpdateCustomer_ShouldAppendNewCompanyToExistingCollection()
        {
            // arrange
            var databaseName = $"user-tests-{Guid.NewGuid():N}";
            await using var serviceProvider = TestServiceProviderFactory.Create(_fixture.ConnectionString, databaseName);
            var repository = serviceProvider.GetRequiredService<ICustomerRepository>();

            var customer = CreateTestCustomer();
            customer.AddCompany("First Company", "TAX-1", CreateDefaultAddress("00-001"), CreateDefaultAddress("00-001"));
            await repository.CreateCustomer(customer, CancellationToken.None);

            var customerToUpdate = await repository.GetByIdAsync(customer.Id, CancellationToken.None);
            customerToUpdate.ShouldNotBeNull();

            customerToUpdate.AddCompany("Second Company", "TAX-2", CreateDefaultAddress("00-002"), CreateDefaultAddress("00-002"));

            // act
            await repository.UpdateCustomer(customerToUpdate, CancellationToken.None);

            // assert
            var updatedResult = await repository.GetByIdAsync(customer.Id, CancellationToken.None);
            updatedResult.ShouldNotBeNull();
            updatedResult.Companies.Count.ShouldBe(2);
            updatedResult.Companies.ShouldContain(x => x.CompanyName == "First Company");
            updatedResult.Companies.ShouldContain(x => x.CompanyName == "Second Company");
        }

        private static Address CreateDefaultAddress(string postalCode) =>
             new(postalCode, "Warszawa", "Testowa", "10", "5");

        private static IndividualData CreateTestIndividualData() =>
            new(
                "Jan",
                "Kowalski",
                "jan.kowalski@test.pl",
                "+48987654321",
                CreateDefaultAddress("00-001"),
                CreateDefaultAddress("00-001"));

        private static Customer CreateTestCustomer() =>
            new($"entra-id|{Guid.NewGuid()}", CreateTestIndividualData());
    }
}
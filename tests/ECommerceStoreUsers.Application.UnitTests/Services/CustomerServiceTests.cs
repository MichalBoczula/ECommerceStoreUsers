using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Services.Concrete.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerceStoreUsers.Application.UnitTests.Services;

public sealed class CustomerServiceTests
{
    [Fact]
    public async Task CreateCustomer_WhenRequestIsValid_ShouldCreateAndReturnCustomerResponse()
    {
        var request = CreateCustomerRequest();
        var validationResult = new ValidationResult();
        var cancellationToken = CancellationToken.None;

        var customerRepositoryMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var customerValidationPolicyMock = new Mock<IValidationPolicy<Customer>>(MockBehavior.Strict);
        var individualValidationPolicyMock = new Mock<IValidationPolicy<IndividualData>>(MockBehavior.Strict);
        var companyValidationPolicyMock = new Mock<IValidationPolicy<CompanyData>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<CustomerService>>(MockBehavior.Loose);

        customerValidationPolicyMock
            .Setup(policy => policy.Validate(It.Is<Customer>(c => c.ExternalId == request.ExternalId)))
            .ReturnsAsync(validationResult);

        customerRepositoryMock
            .Setup(repo => repo.GetByExternalIdAsync(request.ExternalId, cancellationToken))
            .ReturnsAsync((Customer?)null);

        customerRepositoryMock
            .Setup(repo => repo.CreateCustomer(It.Is<Customer>(c => c.ExternalId == request.ExternalId), cancellationToken))
            .ReturnsAsync((Customer customer, CancellationToken _) => customer);

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        var result = await sut.CreateCustomer(request, cancellationToken);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.ExternalId, result.ExternalId);
        Assert.Equal(request.Individual.FirstName, result.Individual.FirstName);

        customerValidationPolicyMock.Verify(policy => policy.Validate(It.IsAny<Customer>()), Times.Once);
        customerRepositoryMock.Verify(repo => repo.GetByExternalIdAsync(request.ExternalId, cancellationToken), Times.Once);
        customerRepositoryMock.Verify(repo => repo.CreateCustomer(It.IsAny<Customer>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateCustomer_WhenCustomerAlreadyExists_ShouldThrowResourceAlreadyExistsException()
    {
        var request = CreateCustomerRequest();
        var validationResult = new ValidationResult();
        var cancellationToken = CancellationToken.None;
        var existingCustomer = new Customer(request.ExternalId, new IndividualData(
            request.Individual.FirstName,
            request.Individual.LastName,
            request.Individual.Email,
            request.Individual.Phone,
            new Domain.AggregatesModel.Customers.ValueObjects.Address("00-111", "City", "Street", "1", "2"),
            new Domain.AggregatesModel.Customers.ValueObjects.Address("00-222", "City2", "Street2", "2", null)));

        var customerRepositoryMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var customerValidationPolicyMock = new Mock<IValidationPolicy<Customer>>(MockBehavior.Strict);
        var individualValidationPolicyMock = new Mock<IValidationPolicy<IndividualData>>(MockBehavior.Strict);
        var companyValidationPolicyMock = new Mock<IValidationPolicy<CompanyData>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<CustomerService>>(MockBehavior.Loose);

        customerValidationPolicyMock
            .Setup(policy => policy.Validate(It.Is<Customer>(c => c.ExternalId == request.ExternalId)))
            .ReturnsAsync(validationResult);

        customerRepositoryMock
            .Setup(repo => repo.GetByExternalIdAsync(request.ExternalId, cancellationToken))
            .ReturnsAsync(existingCustomer);

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        await Assert.ThrowsAsync<ResourceAlreadyExistsException>(() => sut.CreateCustomer(request, cancellationToken));

        customerValidationPolicyMock.Verify(policy => policy.Validate(It.IsAny<Customer>()), Times.Once);
        customerRepositoryMock.Verify(repo => repo.GetByExternalIdAsync(request.ExternalId, cancellationToken), Times.Once);
        customerRepositoryMock.Verify(repo => repo.CreateCustomer(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateCustomer_WhenValidationFails_ShouldThrowValidationException()
    {
        var request = CreateCustomerRequest();
        var invalidResult = new ValidationResult();
        invalidResult.AddValidationError(new ValidationError
        {
            Entity = nameof(Customer),
            Name = "ExternalId",
            Message = "ExternalId is required"
        });

        var customerRepositoryMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var customerValidationPolicyMock = new Mock<IValidationPolicy<Customer>>(MockBehavior.Strict);
        var individualValidationPolicyMock = new Mock<IValidationPolicy<IndividualData>>(MockBehavior.Strict);
        var companyValidationPolicyMock = new Mock<IValidationPolicy<CompanyData>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<CustomerService>>(MockBehavior.Loose);

        customerValidationPolicyMock
            .Setup(policy => policy.Validate(It.Is<Customer>(c => c.ExternalId == request.ExternalId)))
            .ReturnsAsync(invalidResult);

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        await Assert.ThrowsAsync<ValidationException>(() => sut.CreateCustomer(request, CancellationToken.None));

        customerValidationPolicyMock.Verify(policy => policy.Validate(It.IsAny<Customer>()), Times.Once);
        customerRepositoryMock.Verify(repo => repo.GetByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        customerRepositoryMock.Verify(repo => repo.CreateCustomer(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static CreateCustomerRequestDto CreateCustomerRequest()
    {
        return new CreateCustomerRequestDto
        {
            ExternalId = "customer-ext-123",
            Individual = new IndividualDataRequestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
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
            }
        };
    }
}

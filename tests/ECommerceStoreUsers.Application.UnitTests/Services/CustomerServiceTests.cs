using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Services.Concrete.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

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

    [Fact]
    public async Task GetCustomerByExternalId_WhenCustomerExists_ShouldReturnCustomerResponse()
    {
        var request = CreateCustomerRequest();
        var cancellationToken = CancellationToken.None;
        var customer = new Customer(request.ExternalId, new IndividualData(
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

        customerRepositoryMock
            .Setup(repo => repo.GetByExternalIdAsync(request.ExternalId, cancellationToken))
            .ReturnsAsync(customer);

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        var result = await sut.GetCustomerByExternalId(request.ExternalId, cancellationToken);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(customer.Id);
        result.ExternalId.ShouldBe(request.ExternalId);
        result.Individual.FirstName.ShouldBe(request.Individual.FirstName);

        customerRepositoryMock.Verify(repo => repo.GetByExternalIdAsync(request.ExternalId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetCustomerByExternalId_WhenCustomerDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        var externalId = "missing-customer-ext-id";
        var cancellationToken = CancellationToken.None;

        var customerRepositoryMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var customerValidationPolicyMock = new Mock<IValidationPolicy<Customer>>(MockBehavior.Strict);
        var individualValidationPolicyMock = new Mock<IValidationPolicy<IndividualData>>(MockBehavior.Strict);
        var companyValidationPolicyMock = new Mock<IValidationPolicy<CompanyData>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<CustomerService>>(MockBehavior.Loose);

        customerRepositoryMock
            .Setup(repo => repo.GetByExternalIdAsync(externalId, cancellationToken))
            .ReturnsAsync((Customer?)null);

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        await Should.ThrowAsync<ResourceNotFoundException>(() => sut.GetCustomerByExternalId(externalId, cancellationToken));

        customerRepositoryMock.Verify(repo => repo.GetByExternalIdAsync(externalId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetCustomerByExternalId_WhenLoadFailsValidation_ShouldPropagateValidationException()
    {
        var externalId = string.Empty;
        var cancellationToken = CancellationToken.None;
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

        customerRepositoryMock
            .Setup(repo => repo.GetByExternalIdAsync(externalId, cancellationToken))
            .ThrowsAsync(new ValidationException(invalidResult));

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        await Should.ThrowAsync<ValidationException>(() => sut.GetCustomerByExternalId(externalId, cancellationToken));

        customerRepositoryMock.Verify(repo => repo.GetByExternalIdAsync(externalId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateIndividualData_WhenRequestIsValid_ShouldUpdateCustomerAndReturnResponse()
    {
        var clientId = Guid.NewGuid();
        var request = CreateUpdateIndividualDataRequest();
        var validationResult = new ValidationResult();
        var cancellationToken = CancellationToken.None;

        var customer = new Customer("customer-ext-123", new IndividualData(
            "OldName",
            "OldLast",
            "old@example.com",
            "111222333",
            new Domain.AggregatesModel.Customers.ValueObjects.Address("00-111", "City", "Street", "1", "2"),
            new Domain.AggregatesModel.Customers.ValueObjects.Address("00-222", "City2", "Street2", "2", null)));

        var customerRepositoryMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var customerValidationPolicyMock = new Mock<IValidationPolicy<Customer>>(MockBehavior.Strict);
        var individualValidationPolicyMock = new Mock<IValidationPolicy<IndividualData>>(MockBehavior.Strict);
        var companyValidationPolicyMock = new Mock<IValidationPolicy<CompanyData>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<CustomerService>>(MockBehavior.Loose);

        var sequence = new MockSequence();

        emptyGuidValidationPolicyMock
            .InSequence(sequence)
            .Setup(policy => policy.Validate(clientId))
            .ReturnsAsync(validationResult);

        customerRepositoryMock
            .InSequence(sequence)
            .Setup(repo => repo.GetByIdAsync(clientId, cancellationToken))
            .ReturnsAsync(customer);

        individualValidationPolicyMock
            .InSequence(sequence)
            .Setup(policy => policy.Validate(It.Is<IndividualData>(x => x.Email == request.Individual.Email)))
            .ReturnsAsync(validationResult);

        customerRepositoryMock
            .InSequence(sequence)
            .Setup(repo => repo.UpdateCustomer(customer, cancellationToken))
            .ReturnsAsync(customer);

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        var result = await sut.UpdateIndividualData(clientId, request, cancellationToken);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(customer.Id);
        result.Individual.FirstName.ShouldBe(request.Individual.FirstName);
        result.Individual.LastName.ShouldBe(request.Individual.LastName);
        result.Individual.Email.ShouldBe(request.Individual.Email);

        emptyGuidValidationPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        customerRepositoryMock.Verify(repo => repo.GetByIdAsync(clientId, cancellationToken), Times.Once);
        individualValidationPolicyMock.Verify(policy => policy.Validate(It.IsAny<IndividualData>()), Times.Once);
        customerRepositoryMock.Verify(repo => repo.UpdateCustomer(customer, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateIndividualData_WhenValidationFails_ShouldThrowValidationException()
    {
        var clientId = Guid.Empty;
        var request = CreateUpdateIndividualDataRequest();
        var cancellationToken = CancellationToken.None;
        var invalidResult = new ValidationResult();
        invalidResult.AddValidationError(new ValidationError
        {
            Entity = nameof(Guid),
            Name = "clientId",
            Message = "ClientId cannot be empty"
        });

        var customerRepositoryMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var customerValidationPolicyMock = new Mock<IValidationPolicy<Customer>>(MockBehavior.Strict);
        var individualValidationPolicyMock = new Mock<IValidationPolicy<IndividualData>>(MockBehavior.Strict);
        var companyValidationPolicyMock = new Mock<IValidationPolicy<CompanyData>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<CustomerService>>(MockBehavior.Loose);

        emptyGuidValidationPolicyMock
            .Setup(policy => policy.Validate(clientId))
            .ReturnsAsync(invalidResult);

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        await Should.ThrowAsync<ValidationException>(() => sut.UpdateIndividualData(clientId, request, cancellationToken));

        emptyGuidValidationPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        customerRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        customerRepositoryMock.Verify(repo => repo.UpdateCustomer(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateIndividualData_WhenCustomerDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        var clientId = Guid.NewGuid();
        var request = CreateUpdateIndividualDataRequest();
        var cancellationToken = CancellationToken.None;
        var validationResult = new ValidationResult();

        var customerRepositoryMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var customerValidationPolicyMock = new Mock<IValidationPolicy<Customer>>(MockBehavior.Strict);
        var individualValidationPolicyMock = new Mock<IValidationPolicy<IndividualData>>(MockBehavior.Strict);
        var companyValidationPolicyMock = new Mock<IValidationPolicy<CompanyData>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<CustomerService>>(MockBehavior.Loose);

        var sequence = new MockSequence();
        emptyGuidValidationPolicyMock
            .InSequence(sequence)
            .Setup(policy => policy.Validate(clientId))
            .ReturnsAsync(validationResult);

        customerRepositoryMock
            .InSequence(sequence)
            .Setup(repo => repo.GetByIdAsync(clientId, cancellationToken))
            .ReturnsAsync((Customer?)null);

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        await Should.ThrowAsync<ResourceNotFoundException>(() => sut.UpdateIndividualData(clientId, request, cancellationToken));

        emptyGuidValidationPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        customerRepositoryMock.Verify(repo => repo.GetByIdAsync(clientId, cancellationToken), Times.Once);
        individualValidationPolicyMock.Verify(policy => policy.Validate(It.IsAny<IndividualData>()), Times.Never);
        customerRepositoryMock.Verify(repo => repo.UpdateCustomer(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task AddCompany_WhenRequestIsValid_ShouldAddCompanyAndReturnCustomerResponse()
    {
        var clientId = Guid.NewGuid();
        var request = CreateAddCompanyRequest();
        var validationResult = new ValidationResult();
        var cancellationToken = CancellationToken.None;

        var customer = new Customer("customer-ext-123", new IndividualData(
            "John",
            "Doe",
            "john.doe@example.com",
            "111222333",
            new Domain.AggregatesModel.Customers.ValueObjects.Address("00-111", "City", "Street", "1", "2"),
            new Domain.AggregatesModel.Customers.ValueObjects.Address("00-222", "City2", "Street2", "2", null)));

        var customerRepositoryMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var customerValidationPolicyMock = new Mock<IValidationPolicy<Customer>>(MockBehavior.Strict);
        var individualValidationPolicyMock = new Mock<IValidationPolicy<IndividualData>>(MockBehavior.Strict);
        var companyValidationPolicyMock = new Mock<IValidationPolicy<CompanyData>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<CustomerService>>(MockBehavior.Loose);

        var sequence = new MockSequence();

        emptyGuidValidationPolicyMock
            .InSequence(sequence)
            .Setup(policy => policy.Validate(clientId))
            .ReturnsAsync(validationResult);

        customerRepositoryMock
            .InSequence(sequence)
            .Setup(repo => repo.GetByIdAsync(clientId, cancellationToken))
            .ReturnsAsync(customer);

        companyValidationPolicyMock
            .InSequence(sequence)
            .Setup(policy => policy.Validate(It.Is<CompanyData>(x => x.TaxId == request.TaxId && x.CompanyName == request.CompanyName)))
            .ReturnsAsync(validationResult);

        customerRepositoryMock
            .InSequence(sequence)
            .Setup(repo => repo.UpdateCustomer(customer, cancellationToken))
            .ReturnsAsync(customer);

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        var result = await sut.AddCompany(clientId, request, cancellationToken);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(customer.Id);
        var company = result.Companies.ShouldHaveSingleItem();
        company.CompanyName.ShouldBe(request.CompanyName);
        company.TaxId.ShouldBe(request.TaxId);

        emptyGuidValidationPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        customerRepositoryMock.Verify(repo => repo.GetByIdAsync(clientId, cancellationToken), Times.Once);
        companyValidationPolicyMock.Verify(policy => policy.Validate(It.IsAny<CompanyData>()), Times.Once);
        customerRepositoryMock.Verify(repo => repo.UpdateCustomer(customer, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task AddCompany_WhenValidationFails_ShouldThrowValidationException()
    {
        var clientId = Guid.Empty;
        var request = CreateAddCompanyRequest();
        var cancellationToken = CancellationToken.None;
        var invalidResult = new ValidationResult();
        invalidResult.AddValidationError(new ValidationError
        {
            Entity = nameof(Guid),
            Name = "clientId",
            Message = "ClientId cannot be empty"
        });

        var customerRepositoryMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var customerValidationPolicyMock = new Mock<IValidationPolicy<Customer>>(MockBehavior.Strict);
        var individualValidationPolicyMock = new Mock<IValidationPolicy<IndividualData>>(MockBehavior.Strict);
        var companyValidationPolicyMock = new Mock<IValidationPolicy<CompanyData>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<CustomerService>>(MockBehavior.Loose);

        emptyGuidValidationPolicyMock
            .Setup(policy => policy.Validate(clientId))
            .ReturnsAsync(invalidResult);

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        await Should.ThrowAsync<ValidationException>(() => sut.AddCompany(clientId, request, cancellationToken));

        emptyGuidValidationPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        customerRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        customerRepositoryMock.Verify(repo => repo.UpdateCustomer(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddCompany_WhenCustomerDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        var clientId = Guid.NewGuid();
        var request = CreateAddCompanyRequest();
        var cancellationToken = CancellationToken.None;
        var validationResult = new ValidationResult();

        var customerRepositoryMock = new Mock<ICustomerRepository>(MockBehavior.Strict);
        var customerValidationPolicyMock = new Mock<IValidationPolicy<Customer>>(MockBehavior.Strict);
        var individualValidationPolicyMock = new Mock<IValidationPolicy<IndividualData>>(MockBehavior.Strict);
        var companyValidationPolicyMock = new Mock<IValidationPolicy<CompanyData>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<CustomerService>>(MockBehavior.Loose);

        var sequence = new MockSequence();
        emptyGuidValidationPolicyMock
            .InSequence(sequence)
            .Setup(policy => policy.Validate(clientId))
            .ReturnsAsync(validationResult);

        customerRepositoryMock
            .InSequence(sequence)
            .Setup(repo => repo.GetByIdAsync(clientId, cancellationToken))
            .ReturnsAsync((Customer?)null);

        var sut = new CustomerService(
            customerRepositoryMock.Object,
            customerValidationPolicyMock.Object,
            individualValidationPolicyMock.Object,
            companyValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        await Should.ThrowAsync<ResourceNotFoundException>(() => sut.AddCompany(clientId, request, cancellationToken));

        emptyGuidValidationPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        customerRepositoryMock.Verify(repo => repo.GetByIdAsync(clientId, cancellationToken), Times.Once);
        companyValidationPolicyMock.Verify(policy => policy.Validate(It.IsAny<CompanyData>()), Times.Never);
        customerRepositoryMock.Verify(repo => repo.UpdateCustomer(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
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


    private static AddCompanyRequestDto CreateAddCompanyRequest()
    {
        return new AddCompanyRequestDto
        {
            CompanyName = "Acme Corp",
            TaxId = "1234567890",
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
        };
    }

    private static UpdateIndividualDataRequestDto CreateUpdateIndividualDataRequest()
    {
        return new UpdateIndividualDataRequestDto
        {
            Individual = new IndividualDataRequestDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                Phone = "987654321",
                BillingAddress = new AddressRequestDto
                {
                    PostalCode = "10-100",
                    City = "Gdansk",
                    Street = "Long",
                    BuildingNumber = "5",
                    ApartmentNumber = "11"
                },
                ShippingAddress = new AddressRequestDto
                {
                    PostalCode = "20-200",
                    City = "Wroclaw",
                    Street = "Short",
                    BuildingNumber = "7",
                    ApartmentNumber = "12"
                }
            }
        };
    }
}

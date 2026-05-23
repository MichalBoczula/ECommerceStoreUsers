using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Application.Descriptors.Customers;
using ECommerceStoreUsers.Application.Services.Abstract.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.Extensions.Logging;

namespace ECommerceStoreUsers.Application.Services.Concrete.Customers
{
    internal sealed class CustomerService(
     ICustomerRepository _customerRepository,
     IValidationPolicy<Customer> _customerValidationPolicy,
     IValidationPolicy<IndividualData> _individualDataValidationPolicy,
     IValidationPolicy<Guid> _emptyGuidValidationPolicy,
     ILogger<CustomerService> _logger)
     : ICustomerService
    {
        public async Task<CustomerResponseDto> CreateCustomer(CreateCustomerRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating customer creation flow for ExternalId: {ExternalId}", request.ExternalId);

            var descriptor = new CreateCustomerDescriptor();

            var customer = descriptor.MapToDomain(request);

            var validationResult = await descriptor.ValidateCustomer(customer, _customerValidationPolicy);
            descriptor.ThrowValidationExceptionIfCustomerInvalid(validationResult);

            var existingCustomer = await descriptor.LoadCustomer(customer.ExternalId, _customerRepository, cancellationToken);
            descriptor.ThrowAlreadyExistsExceptionIfCustomerExists(customer.ExternalId, existingCustomer);

            var createdCustomer = await descriptor.Save(customer, _customerRepository, cancellationToken);

            var response = descriptor.MapToResponse(createdCustomer);

            _logger.LogInformation("Successfully created customer profile. CustomerId: {CustomerId} for ExternalId: {ExternalId}", response.Id, request.ExternalId);

            return response;
        }

        public async Task<CustomerResponseDto> GetCustomerByExternalId(string externalId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating get customer by ExternalId flow for ExternalId: {ExternalId}", externalId);

            var descriptor = new GetCustomerByExternalIdDescriptor();

            var customer = await descriptor.LoadCustomer(externalId, _customerRepository, cancellationToken);
            descriptor.ThrowNotFoundExceptionIfCustomerMissing(externalId, customer);

            var response = descriptor.MapToResponse(customer!);

            _logger.LogInformation("Successfully loaded customer profile. CustomerId: {CustomerId} for ExternalId: {ExternalId}", response.Id, externalId);

            return response;
        }

        public async Task<CustomerResponseDto> UpdateIndividualData(Guid clientId, UpdateIndividualDataRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating update individual data flow for CustomerId: {CustomerId}", clientId);

            var descriptor = new UpdateIndividualDataDescriptor();

            var customerIdValidationResult = await descriptor.ValidateCustomerId(clientId, _emptyGuidValidationPolicy);
            descriptor.ThrowValidationExceptionIfCustomerIdInvalid(customerIdValidationResult);

            var customer = await descriptor.LoadCustomer(clientId, _customerRepository, cancellationToken);
            descriptor.ThrowNotFoundExceptionIfCustomerMissing(clientId, customer);

            var individualData = descriptor.MapRequestToIndividualData(request);
            descriptor.UpdateCustomerIndividual(customer!, individualData);

            var validationResult = await descriptor.ValidateIndividualData(individualData, _individualDataValidationPolicy);
            descriptor.ThrowValidationExceptionIfIndividualDataInvalid(validationResult);

            var updatedCustomer = await descriptor.Save(customer!, _customerRepository, cancellationToken);

            var response = descriptor.MapToResponse(updatedCustomer);

            _logger.LogInformation("Successfully updated individual data. CustomerId: {CustomerId}", clientId);

            return response;
        }

        public async Task<CustomerResponseDto> AddCompany(Guid clientId, AddCompanyRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating add company flow for CustomerId: {CustomerId}", clientId);

            var descriptor = new AddCompanyDescriptor();

            var customerIdValidationResult = await descriptor.ValidateCustomerId(clientId, _emptyGuidValidationPolicy);
            descriptor.ThrowValidationExceptionIfCustomerIdInvalid(customerIdValidationResult);

            var customer = await descriptor.LoadCustomer(clientId, _customerRepository, cancellationToken);
            descriptor.ThrowNotFoundExceptionIfCustomerMissing(clientId, customer);

            var billingAddress = descriptor.MapAddress(request.BillingAddress);
            var shippingAddress = descriptor.MapAddress(request.ShippingAddress);
            descriptor.AddCompanyToCustomer(customer!, request, billingAddress, shippingAddress);

            var validationResult = await descriptor.ValidateCustomer(customer!, _customerValidationPolicy);
            descriptor.ThrowValidationExceptionIfCustomerInvalid(validationResult);

            var updatedCustomer = await descriptor.Save(customer!, _customerRepository, cancellationToken);

            var response = descriptor.MapToResponse(updatedCustomer);

            _logger.LogInformation("Successfully added company data. CustomerId: {CustomerId}", clientId);

            return response;
        }

        public async Task<CustomerResponseDto> UpdateCompany(Guid clientId, Guid companyId, UpdateCompanyRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating update company flow for CustomerId: {CustomerId}, CompanyId: {CompanyId}", clientId, companyId);

            var descriptor = new UpdateCompanyDescriptor();

            var customerIdValidationResult = await descriptor.ValidateCustomerId(clientId, _emptyGuidValidationPolicy);
            descriptor.ThrowValidationExceptionIfCustomerIdInvalid(customerIdValidationResult);

            var companyIdValidationResult = await descriptor.ValidateCompanyId(companyId, _emptyGuidValidationPolicy);
            descriptor.ThrowValidationExceptionIfCompanyIdInvalid(companyIdValidationResult);

            var customer = await descriptor.LoadCustomer(clientId, _customerRepository, cancellationToken);
            descriptor.ThrowNotFoundExceptionIfCustomerMissing(clientId, customer);

            var billingAddress = descriptor.MapAddress(request.BillingAddress);
            var shippingAddress = descriptor.MapAddress(request.ShippingAddress);
            var company = descriptor.LoadCompany(customer!, companyId);
            descriptor.ThrowNotFoundExceptionIfCompanyMissing(companyId, company);
            descriptor.UpdateCompanyInsideAggregate(company!, request, billingAddress, shippingAddress);

            var validationResult = await descriptor.ValidateCustomer(customer!, _customerValidationPolicy);
            descriptor.ThrowValidationExceptionIfCustomerInvalid(validationResult);

            var updatedCustomer = await descriptor.Save(customer!, _customerRepository, cancellationToken);

            var response = descriptor.MapToResponse(updatedCustomer);

            _logger.LogInformation("Successfully updated company data. CustomerId: {CustomerId}, CompanyId: {CompanyId}", clientId, companyId);

            return response;
        }
    }
}

using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using Microsoft.Extensions.Logging;

namespace ECommerceStoreUsers.Application.Services.Customers
{
    internal sealed class CustomerService(
     ICustomerRepository customerRepository,
     IValidationPolicy<Customer> _customerValidationPolicy,
     ILogger<CustomerService> logger)
     : ICustomerService
    {
        //public async Task<CustomerResponseDto> CreateCustomer(CreateCustomerRequestDto request, CancellationToken cancellationToken)
        //{
        //    logger.LogInformation("Initiating customer creation flow for ExternalId: {ExternalId}", request.ExternalId);

        //    var customer = MappingConfig.MapToDomain(request);

        //    var validationResult1 = await _customerValidationPolicy.Validate(customer);

        //    //var validationResult = await descriptor.ValidateExternalId(request.ExternalId, _stringValidationPolicy);
        //    //descriptor.ThrowValidationExceptionIfExternalIdInvalid(validationResult);

        //    //var existingCustomer = await descriptor.LoadCustomer(request.ExternalId, customerRepository);
        //    //descriptor.ThrowAlreadyExistsExceptionIfCustomerExists(request.ExternalId, existingCustomer);

        //    //var individualData = descriptor.MapRequestToIndividualData(request);
        //    //validationResult = await descriptor.ValidateIndividualData(individualData, _individualDataValidationPolicy);
        //    //descriptor.ThrowValidationExceptionIfIndividualDataInvalid(validationResult);

        //    //var customer = descriptor.Create(request.ExternalId, individualData);
        //    //var createdCustomer = await descriptor.SaveCustomer(customer, customerRepository);

        //    //var response = descriptor.MapToResponse(createdCustomer);
        //    logger.LogInformation("Successfully created customer profile. CustomerId: {CustomerId} for ExternalId: {ExternalId}", response.Id, request.ExternalId);

        //    return response;
        //}
        //public async Task<CustomerResponseDto> GetCustomerByExternalId(string externalId)
        //{
        //    logger.LogInformation("Processing customer read request for ExternalId: {ExternalId}", externalId, CancellationToken cancellationToken);

        //    var descriptor = new GetCustomerByExternalIdDescriptor();

        //    var validationResult = await descriptor.ValidateExternalId(externalId, _stringValidationPolicy);
        //    descriptor.ThrowValidationExceptionIfExternalIdInvalid(validationResult);

        //    var customer = await descriptor.LoadCustomer(externalId, customerRepository);
        //    descriptor.ThrowNotFoundExceptionIfCustomerMissing(externalId, customer);

        //    var response = descriptor.MapToResponse(customer!);
        //    logger.LogInformation("Successfully retrieved customer profile for ExternalId: {ExternalId}, CustomerId: {CustomerId}", externalId, response.Id);

        //    return response;
        //}


        //public async Task<CustomerResponseDto> UpdateIndividualData(string customerId, UpdateIndividualDataRequestDto request, CancellationToken cancellationToken)
        //{
        //    logger.LogInformation("Initiating individual data update flow for CustomerId: {CustomerId}", customerId);

        //    var descriptor = new UpdateIndividualDataDescriptor();

        //    var validationResult = await descriptor.ValidateCustomerId(customerId, _guidValidationPolicy);
        //    descriptor.ThrowValidationExceptionIfCustomerIdInvalid(validationResult);

        //    var customer = await descriptor.LoadCustomer(customerId, customerRepository,);
        //    descriptor.ThrowNotFoundExceptionIfCustomerMissing(customerId, customer);

        //    var newIndividualData = descriptor.MapRequestToIndividualData(request);
        //    validationResult = await descriptor.ValidateIndividualData(newIndividualData, _individualDataValidationPolicy);
        //    descriptor.ThrowValidationExceptionIfIndividualDataInvalid(validationResult);

        //    descriptor.UpdateCustomerIndividual(customer!, newIndividualData);

        //    var updatedCustomer = await descriptor.SaveCustomer(customer!, customerRepository);
        //    var response = descriptor.MapToResponse(updatedCustomer);

        //    logger.LogInformation("Successfully updated individual data. CustomerId: {CustomerId}", response.Id);

        //    return response;
        //}

        //public async Task<CustomerResponseDto> AddCompany(string customerId, AddCompanyRequestDto request, CancellationToken cancellationToken)
        //{
        //    logger.LogInformation("Initiating company registration flow for CustomerId: {CustomerId}", customerId);

        //    var descriptor = new AddCompanyDescriptor();

        //    var validationResult = await descriptor.ValidateCustomerId(customerId, _guidValidationPolicy);
        //    descriptor.ThrowValidationExceptionIfCustomerIdInvalid(validationResult);

        //    var customer = await descriptor.LoadCustomer(customerId, customerRepository, cancellationToken);
        //    descriptor.ThrowNotFoundExceptionIfCustomerMissing(customerId, customer);

        //    var billingAddress = descriptor.MapAddress(request.BillingAddress);
        //    var shippingAddress = descriptor.MapAddress(request.ShippingAddress);

        //    customer!.AddCompany(request.CompanyName, request.TaxId, billingAddress, shippingAddress);

        //    var updatedCustomer = await descriptor.SaveCustomer(customer, customerRepository, cancellationToken);
        //    var response = descriptor.MapToResponse(updatedCustomer);

        //    logger.LogInformation("Successfully added company to CustomerId: {CustomerId}", customerId);
        //    return response;
        //}

        //public async Task<CustomerResponseDto> UpdateCompany(string externalId, Guid companyId, UpdateCompanyRequestDto request, CancellationToken cancellationToken)
        //{
        //    logger.LogInformation("Initiating company modification flow for CompanyId: {CompanyId} under CustomerId: {CustomerId}", companyId, externalId);

        //    var descriptor = new UpdateCompanyDescriptor();

        //    var companyValidation = await descriptor.ValidateCompanyId(companyId, _guidValidationPolicy);
        //    descriptor.ThrowValidationExceptionIfIdentifiersInvalid(companyValidation);

        //    var customer = await descriptor.LoadCustomer(externalId, customerRepository, cancellationToken);
        //    descriptor.ThrowNotFoundExceptionIfCustomerMissing(externalId, customer);

        //    var billingAddress = descriptor.MapAddress(request.BillingAddress);
        //    var shippingAddress = descriptor.MapAddress(request.ShippingAddress);

        //    descriptor.UpdateCompanyInsideAggregate(customer!, companyId, request.TaxId, request.CompanyName, billingAddress, shippingAddress);

        //    var updatedCustomer = await descriptor.SaveCustomer(customer!, customerRepository);
        //    var response = descriptor.MapToResponse(updatedCustomer);

        //    logger.LogInformation("Successfully modified company context for CompanyId: {CompanyId}", companyId);
        //    return response;
        //}
        public Task<CustomerResponseDto> GetCustomerByExternalId(string externalId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerResponseDto> CreateCustomer(CreateCustomerRequestDto request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerResponseDto> UpdateIndividualData(Guid id, UpdateIndividualDataRequestDto request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerResponseDto> AddCompany(string externalId, AddCompanyRequestDto request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerResponseDto> UpdateCompany(string externalId, Guid companyId, UpdateCompanyRequestDto request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

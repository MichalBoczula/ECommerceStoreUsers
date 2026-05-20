using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Application.Descriptors.Customers;
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
        public async Task<CustomerResponseDto> CreateCustomer(CreateCustomerRequestDto request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Initiating customer creation flow for ExternalId: {ExternalId}", request.ExternalId);

            var descriptor = new CreateCustomerDescriptor();

            var customer = descriptor.MapToDomain(request);

            var validationResult = await descriptor.ValidateCustomer(customer, _customerValidationPolicy);
            descriptor.ThrowValidationExceptionIfCustomerInvalid(validationResult);

            var existingCustomer = await descriptor.LoadCustomer(customer.ExternalId, customerRepository, cancellationToken);
            descriptor.ThrowAlreadyExistsExceptionIfCustomerExists(customer.ExternalId, existingCustomer);

            var createdCustomer = await descriptor.Save(customer, customerRepository, cancellationToken);

            var response = descriptor.MapToResponse(createdCustomer);

            logger.LogInformation("Successfully created customer profile. CustomerId: {CustomerId} for ExternalId: {ExternalId}", response.Id, request.ExternalId);

            return response;
        }

        public Task<CustomerResponseDto> GetCustomerByExternalId(string externalId, CancellationToken cancellationToken)
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

using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Customers
{
    internal sealed record GetCustomerByExternalId;

    internal sealed class GetCustomerByExternalIdDescriptor : FlowDescriberBase<GetCustomerByExternalId>
    {
        [FlowStep(order: 1, bpmnId: "MapExternalIdToCustomerAggregate")]
        public Customer MapExternalIdToCustomer(string externalId)
        {
            var validationAddress = new Address("00-001", "Validation City", "Validation Street", "1", null);
            var validationIndividual = new IndividualData(
                "Validation",
                "Customer",
                "validation.customer@example.com",
                "123456789",
                validationAddress,
                validationAddress);

            return new Customer(externalId, validationIndividual);
        }

        [FlowStep(order: 2, bpmnId: "ValidateCustomerExternalId")]
        public async Task<ValidationResult> ValidateCustomer(Customer customer, IValidationPolicy<Customer> customerValidationPolicy)
        {
            return await customerValidationPolicy.Validate(customer);
        }

        [FlowStep(order: 3, bpmnId: "IsCustomerExternalIdValid")]
        public void ThrowValidationExceptionIfCustomerInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 4, bpmnId: "LoadCustomerProfile")]
        public async Task<Customer?> LoadCustomer(string externalId, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.GetByExternalIdAsync(externalId, cancellationToken);
        }

        [FlowStep(order: 5, bpmnId: "VerifyCustomerExists")]
        public void ThrowNotFoundExceptionIfCustomerMissing(string externalId, Customer? customer)
        {
            if (customer is null)
            {
                throw new ResourceNotFoundException(
                    nameof(GetCustomerByExternalId),
                    externalId,
                    nameof(Customer));
            }
        }

        [FlowStep(order: 6, bpmnId: "MapCustomerResponse")]
        public CustomerResponseDto MapToResponse(Customer customer)
        {
            return MappingConfig.MapToResponse(customer);
        }
    }
}

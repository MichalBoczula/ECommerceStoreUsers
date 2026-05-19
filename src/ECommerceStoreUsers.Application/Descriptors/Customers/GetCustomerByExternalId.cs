using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Customers
{
    internal sealed record GetCustomerByExternalId;

    internal sealed class GetCustomerByExternalIdDescriptor : FlowDescriberBase<GetCustomerByExternalId>
    {
        [FlowStep(order: 1, bpmnId: "ValidateExternalId")]
        public async Task<ValidationResult> ValidateExternalId(string externalId, IValidationPolicy<string> stringValidationPolicy)
        {
            return await stringValidationPolicy.Validate(externalId);
        }

        [FlowStep(order: 2, bpmnId: "IsExternalIdValid")]
        public void ThrowValidationExceptionIfExternalIdInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 3, bpmnId: "LoadCustomerProfile")]
        public async Task<Customer?> LoadCustomer(string externalId, ICustomerRepository customerRepository)
        {
            return await customerRepository.GetByExternalIdAsync(externalId);
        }

        [FlowStep(order: 4, bpmnId: "VerifyCustomerExists")]
        public void ThrowNotFoundExceptionIfCustomerMissing(string externalId, Customer? customer)
        {
            if (customer is null)
            {
                throw new ResourceNotFoundException(nameof(LoadCustomer), externalId, nameof(Customer));
            }
        }

        [FlowStep(order: 5, bpmnId: "MapCustomerResponse")]
        public CustomerResponseDto MapToResponse(Customer customer)
        {
            return CustomerMappingDto.MapToResponse(customer);
        }
    }
}

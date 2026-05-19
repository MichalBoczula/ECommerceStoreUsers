using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Customers
{
    internal sealed record AddCompany;

    internal sealed class AddCompanyDescriptor : FlowDescriberBase<AddCompany>
    {
        [FlowStep(order: 1, bpmnId: "ValidateCustomerId")]
        public async Task<ValidationResult> ValidateCustomerId(string id, IValidationPolicy<Guid> guidValidationPolicy)
        {
            return await guidValidationPolicy.Validate(id);
        }

        [FlowStep(order: 2, bpmnId: "IsCustomerIdValid")]
        public void ThrowValidationExceptionIfCustomerIdInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 3, bpmnId: "LoadCustomerProfile")]
        public async Task<Customer?> LoadCustomer(string customerId, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.GetByExternalIdAsync(customerId, cancellationToken);
        }

        [FlowStep(order: 4, bpmnId: "VerifyCustomerExists")]
        public void ThrowNotFoundExceptionIfCustomerMissing(Guid id, Customer? customer)
        {
            if (customer is null)
            {
                throw new ResourceNotFoundException(nameof(LoadCustomer), id, nameof(Customer));
            }
        }

        [FlowStep(order: 5, bpmnId: "MapAddress")]
        public Address MapAddress(AddressRequestDto addressDto)
        {
            return CustomerMappingDto.MapAddress(addressDto);
        }

        [FlowStep(order: 6, bpmnId: "SaveCustomer")]
        public async Task<Customer> SaveCustomer(Customer customer, ICustomerRepository customerRepository)
        {
            return await customerRepository.UpdateCustomer(customer);
        }

        [FlowStep(order: 7, bpmnId: "MapCustomerResponse")]
        public CustomerResponseDto MapToResponse(Customer customer)
        {
            return CustomerMappingDto.MapToResponse(customer);
        }
    }
}

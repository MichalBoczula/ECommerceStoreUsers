using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Application.Mapping;
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
        [FlowStep(order: 1, bpmnId: "LoadCustomerProfile")]
        public async Task<Customer?> LoadCustomer(Guid customerId, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.GetByIdAsync(customerId, cancellationToken);
        }

        [FlowStep(order: 2, bpmnId: "VerifyCustomerExists")]
        public void ThrowNotFoundExceptionIfCustomerMissing(Guid customerId, Customer? customer)
        {
            if (customer is null)
            {
                throw new ResourceNotFoundException(
                    nameof(AddCompany),
                    customerId.ToString(),
                    nameof(Customer));
            }
        }

        [FlowStep(order: 3, bpmnId: "MapAddress")]
        public Address MapAddress(AddressRequestDto address)
        {
            return MappingConfig.MapAddress(address);
        }

        [FlowStep(order: 4, bpmnId: "AddCompanyToCustomer")]
        public void AddCompanyToCustomer(Customer customer, AddCompanyRequestDto request, Address billing, Address shipping)
        {
            customer.AddCompany(request.CompanyName, request.TaxId, billing, shipping);
        }

        [FlowStep(order: 5, bpmnId: "ValidateCustomerAggregate")]
        public async Task<ValidationResult> ValidateCustomer(Customer customer, IValidationPolicy<Customer> customerValidationPolicy)
        {
            return await customerValidationPolicy.Validate(customer);
        }

        [FlowStep(order: 6, bpmnId: "IsCustomerAggregateValid")]
        public void ThrowValidationExceptionIfCustomerInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 7, bpmnId: "SaveCustomer")]
        public async Task<Customer> Save(Customer customer, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.UpdateCustomer(customer, cancellationToken);
        }

        [FlowStep(order: 8, bpmnId: "MapCustomerResponse")]
        public CustomerResponseDto MapToResponse(Customer customer)
        {
            return MappingConfig.MapToResponse(customer);
        }
    }
}

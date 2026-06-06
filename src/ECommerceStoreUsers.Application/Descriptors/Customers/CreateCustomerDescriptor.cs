using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Customers
{
    internal sealed record CreateCustomer;

    internal sealed class CreateCustomerDescriptor : FlowDescriberBase<CreateCustomer>
    {
        [FlowStep(order: 1, bpmnId: "MapRequestToDomain")]
        public Customer MapToDomain(CreateCustomerRequestDto request)
        {
            return MappingConfig.MapToDomain(request);
        }

        [FlowStep(order: 2, bpmnId: "ValidateCustomerAggregate")]
        public async Task<ValidationResult> ValidateCustomer(Customer customer, IValidationPolicy<Customer> customerValidationPolicy)
        {
            return await customerValidationPolicy.Validate(customer);
        }

        [FlowStep(order: 3, bpmnId: "IsCustomerAggregateValid")]
        public void ThrowValidationExceptionIfCustomerInvalid(Customer customer, ValidationResult validationResult)
        {
            if (validationResult.IsValid)
            {
                return;
            }

            if (validationResult.GetValidationErrors().Any(error => error.Name == "CustomerCompanyTaxIdValidationRule"))
            {
                throw new ResourceAlreadyExistsException(
                    nameof(CreateCustomer),
                    string.Join(",", customer.Companies.Select(company => company.TaxId).Where(taxId => !string.IsNullOrWhiteSpace(taxId)).Distinct()),
                    nameof(CompanyData));
            }

            throw new ValidationException(validationResult);
        }

        [FlowStep(order: 4, bpmnId: "LoadExistingCustomerByExternalId")]
        public async Task<Customer?> LoadCustomer(string externalId, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.GetByExternalIdAsync(externalId, cancellationToken);
        }

        [FlowStep(order: 5, bpmnId: "IsCustomerAlreadyExists")]
        public void ThrowAlreadyExistsExceptionIfCustomerExists(string externalId, Customer? customer)
        {
            if (customer is not null)
            {
                throw new ResourceAlreadyExistsException(
                    "CreateCustomer",
                    externalId,
                    nameof(Customer));
            }
        }

        [FlowStep(order: 6, bpmnId: "SaveCustomer")]
        public async Task<Customer> Save(Customer customer, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.CreateCustomer(customer, cancellationToken);
        }

        [FlowStep(order: 7, bpmnId: "MapCustomerResponse")]
        public CustomerResponseDto MapToResponse(Customer customer)
        {
            return MappingConfig.MapToResponse(customer);
        }
    }
}

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
    internal sealed record UpdateIndividualData;

    internal sealed class UpdateIndividualDataDescriptor : FlowDescriberBase<UpdateIndividualData>
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
        public async Task<Customer?> LoadCustomer(string id, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.GetByExternalIdAsync(id, cancellationToken);
        }

        [FlowStep(order: 4, bpmnId: "VerifyCustomerExists")]
        public void ThrowNotFoundExceptionIfCustomerMissing(Guid id, Customer? customer)
        {
            if (customer is null)
            {
                throw new ResourceNotFoundException(nameof(LoadCustomer), id, nameof(Customer));
            }
        }

        [FlowStep(order: 5, bpmnId: "MapRequestToIndividualData")]
        public IndividualData MapRequestToIndividualData(UpdateIndividualDataRequestDto request)
        {
            return MappingConfig.MapToDomain(request.Individual);
        }

        [FlowStep(order: 6, bpmnId: "ValidateIndividualData")]
        public async Task<ValidationResult> ValidateIndividualData(IndividualData individualData, IValidationPolicy<IndividualData> validationPolicy)
        {
            return await validationPolicy.Validate(individualData);
        }

        [FlowStep(order: 7, bpmnId: "IsIndividualDataValid")]
        public void ThrowValidationExceptionIfIndividualDataInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 8, bpmnId: "UpdateCustomerIndividual")]
        public void UpdateCustomerIndividual(Customer customer, IndividualData individualData)
        {
            customer.UpdateIndividualData(individualData);
        }

        [FlowStep(order: 9, bpmnId: "SaveCustomer")]
        public async Task<Customer> SaveCustomer(Customer customer, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.UpdateCustomer(customer, cancellationToken);
        }

        [FlowStep(order: 10, bpmnId: "MapCustomerResponse")]
        public CustomerResponseDto MapToResponse(Customer customer)
        {
            return MappingConfig.MapToResponse(customer);
        }
    }
}

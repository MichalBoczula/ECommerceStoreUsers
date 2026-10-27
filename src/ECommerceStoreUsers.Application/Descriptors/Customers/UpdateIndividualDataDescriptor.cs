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

    [Documentation("users.flow.update-individual-data", "Replace Customer individual data and atomically append the next current/history version.")]
    [FlowOutcome("users.branch.update-individual-data.saved", "Valid input passes checks and the repository commits the requested write.", "Return the complete 200 aggregate response with atomic current/history at the resulting version.", Status = 200, Stage = "application")]
    [FlowOutcome("users.branch.update-individual-data.missing", "The aggregate is absent when initially loaded.", "Return 404 resource_not_found before saving.", Status = 404, Code = "resource_not_found", Stage = "application")]
    [FlowOutcome("users.branch.update-individual-data.removed-before-save", "A competing write removes the loaded aggregate before persistence.", "Return 404 resource_not_found; preserve the competing deletion and all unrelated state.", Status = 404, Code = "resource_not_found", Stage = "persistence")]
    [FlowOutcome("users.branch.update-individual-data.write-conflict", "The repository detects a stale version or MongoDB write conflict 112.", "Abort the losing transaction and return 409 concurrency_conflict; retain the committed winner.", Status = 409, Code = "concurrency_conflict", Stage = "persistence")]
    [FlowOutcome("users.branch.update-individual-data.validation", "An invoked policy collects errors and its validation guard rejects them.", "Return 400 validation_failed with complete rule errors and no writes.", "users.rule.address-apartment-number", "users.rule.address-building-number", "users.rule.address-city", "users.rule.address-postal-code", "users.rule.address-street", "users.rule.empty-guid", "users.rule.individual-email", "users.rule.individual-first-name", "users.rule.individual-last-name", "users.rule.individual-phone", Status = 400, Code = "validation_failed", Stage = "application")]
    [FlowOutcome("users.branch.update-individual-data.required-object", "A bound required individual/address object is explicitly null.", "Return 400 validation_failed with RequiredRequestObject before persistence.", "users.guard.required-request-object", Status = 400, Code = "validation_failed", Stage = "application")]
    internal sealed class UpdateIndividualDataDescriptor : FlowDescriberBase<UpdateIndividualData>
    {
        [FlowStep(order: 1, bpmnId: "ValidateCustomerId", description: "Run EmptyGuidValidationPolicy for CustomerId; collect errors for the next guard without checking resource existence.")]
        public async Task<ValidationResult> ValidateCustomerId(Guid customerId, IValidationPolicy<Guid> emptyGuidValidationPolicy)
        {
            return await emptyGuidValidationPolicy.Validate(customerId);
        }

        [FlowStep(order: 2, bpmnId: "IsCustomerIdValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfCustomerIdInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 3, bpmnId: "LoadCustomerProfile", description: "Read the Customer by resource UUID; the following existence/conflict guard handles null or an existing match. Repository failures/cancellation stop the flow without this step writing.")]
        public async Task<Customer?> LoadCustomer(Guid customerId, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.GetByIdAsync(customerId, cancellationToken);
        }

        [FlowStep(order: 4, bpmnId: "VerifyCustomerExists", description: "Continue when the requested Customer exists; otherwise throw 404 resource_not_found before persistence.")]
        public void ThrowNotFoundExceptionIfCustomerMissing(Guid customerId, Customer? customer)
        {
            if (customer is null)
            {
                throw new ResourceNotFoundException(
                    nameof(UpdateIndividualData),
                    customerId.ToString(),
                    nameof(Customer));
            }
        }

        [FlowStep(order: 5, bpmnId: "MapRequestToIndividualData", description: "Map the replacement individual and addresses; explicit null required objects produce RequiredRequestObject / 400 validation_failed before writing.", RuleId = "users.guard.required-request-object", RuleName = "RequiredRequestObject")]
        public IndividualData MapRequestToIndividualData(UpdateIndividualDataRequestDto request)
        {
            return MappingConfig.MapToDomain(request.Individual);
        }

        [FlowStep(order: 6, bpmnId: "UpdateCustomerIndividual", description: "Replace the individual on the in-memory Customer and update its timestamp. This path has no general unchanged-PUT no-op promise; saving happens later.")]
        public void UpdateCustomerIndividual(Customer customer, IndividualData individualData)
        {
            customer.UpdateIndividualData(individualData);
        }

        [FlowStep(order: 7, bpmnId: "ValidateIndividualData", description: "Run IndividualDataValidationPolicy and collect its active rules/errors without writing; the next validation guard decides whether to return a safe failure.")]
        public async Task<ValidationResult> ValidateIndividualData(IndividualData individualData, IValidationPolicy<IndividualData> individualDataValidationPolicy)
        {
            return await individualDataValidationPolicy.Validate(individualData);
        }

        [FlowStep(order: 8, bpmnId: "IsIndividualDataValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfIndividualDataInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 9, bpmnId: "SaveCustomer", description: "Atomically replace the Customer at version+1 and append Update history. A removed target maps to 404; stale versions or Mongo write conflict 112 map to 409 concurrency_conflict. Failed transactions abort without committing partial state.")]
        public async Task<Customer> Save(Customer customer, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.UpdateCustomer(customer, cancellationToken);
        }

        [FlowStep(order: 10, bpmnId: "MapCustomerResponse", description: "Map the current Customer into the complete response DTO; this mapping adds no database writes or history.")]
        public CustomerResponseDto MapToResponse(Customer customer)
        {
            return MappingConfig.MapToResponse(customer);
        }
    }
}


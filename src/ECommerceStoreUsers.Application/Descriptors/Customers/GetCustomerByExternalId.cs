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

    [Documentation("users.flow.get-customer-by-external-id", "Read a Customer by external identity without changing stored profile or history.")]
    [FlowOutcome("users.branch.get-customer-by-external-id.read", "The input is valid and the requested result is available.", "Return the complete read-only response with no stored changes.", Status = 200, Stage = "application")]
    [FlowOutcome("users.branch.get-customer-by-external-id.missing", "No profile exists for the external reference.", "Return 404 resource_not_found; leave all collections unchanged.", Status = 404, Code = "resource_not_found", Stage = "application")]
    [FlowOutcome("users.branch.get-customer-by-external-id.validation", "An invoked policy collects errors and its validation guard rejects them.", "Return 400 validation_failed with complete rule errors and no writes. GET external-ID validation uses synthetic valid non-key fields.", "users.rule.customer-external-id", Status = 400, Code = "validation_failed", Stage = "application")]
    internal sealed class GetCustomerByExternalIdDescriptor : FlowDescriberBase<GetCustomerByExternalId>
    {
        [FlowStep(order: 1, bpmnId: "MapExternalIdToCustomerAggregate", description: "Build an in-memory Customer carrying the requested externalId and valid synthetic individual/address data; no stored profile or company data is changed.")]
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

        [FlowStep(order: 2, bpmnId: "ValidateCustomerExternalId", description: "Run CustomerValidationPolicy and collect its active rules/errors without writing; the next validation guard decides whether to return a safe failure.")]
        public async Task<ValidationResult> ValidateCustomer(Customer customer, IValidationPolicy<Customer> customerValidationPolicy)
        {
            return await customerValidationPolicy.Validate(customer);
        }

        [FlowStep(order: 3, bpmnId: "IsCustomerExternalIdValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfCustomerInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 4, bpmnId: "LoadCustomerProfile", description: "Read the Customer by externalId; the following existence/conflict guard handles null or an existing match. Repository failures/cancellation stop the flow without this step writing.")]
        public async Task<Customer?> LoadCustomer(string externalId, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.GetByExternalIdAsync(externalId, cancellationToken);
        }

        [FlowStep(order: 5, bpmnId: "VerifyCustomerExists", description: "Continue when the requested Customer exists; otherwise throw 404 resource_not_found before persistence.")]
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

        [FlowStep(order: 6, bpmnId: "MapCustomerResponse", description: "Map the current Customer into the complete response DTO; this mapping adds no database writes or history.")]
        public CustomerResponseDto MapToResponse(Customer customer)
        {
            return MappingConfig.MapToResponse(customer);
        }
    }
}


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

    [Documentation("users.flow.create-customer", "Create a Customer with individual/company values and atomic version-zero Insert history.")]
    [FlowOutcome("users.branch.create-customer.saved", "Valid input passes checks and the repository commits the requested write.", "Return the complete 200 aggregate response with atomic current/history at the resulting version.", Status = 200, Stage = "application")]
    [FlowOutcome("users.branch.create-customer.external-exists", "The external reference already resolves to a profile during the pre-insert read.", "Return 409 resource_conflict before creating current or history.", Status = 409, Code = "resource_conflict", Stage = "application")]
    [FlowOutcome("users.branch.create-customer.external-race", "A concurrent insert wins the named ExternalId unique key and its committed target is confirmed after abort.", "Return 409 resource_conflict and retain only the winning current/Insert history.", Status = 409, Code = "resource_conflict", Stage = "persistence")]
    [FlowOutcome("users.branch.create-customer.duplicate-company", "Two companies within the same Customer would share the tax ID.", "Return 409 resource_conflict before saving; the translated conflict has no validation error array entries.", "users.rule.company-tax-id-uniqueness", Status = 409, Code = "resource_conflict", Stage = "application")]
    [FlowOutcome("users.branch.create-customer.overlap", "Concurrent create requests overlap; the losing insert hits a confirmed ExternalId key or write conflict 112.", "Abort the losing transaction and return 409 resource_conflict for a confirmed unique-key collision or concurrency_conflict for a write conflict; retain the committed winner.", Status = 409, Code = "resource_conflict|concurrency_conflict", Stage = "persistence")]
    [FlowOutcome("users.branch.create-customer.validation", "An invoked policy collects errors and its validation guard rejects them.", "Return 400 validation_failed with complete rule errors and no writes.", "users.rule.address-apartment-number", "users.rule.address-building-number", "users.rule.address-city", "users.rule.address-postal-code", "users.rule.address-street", "users.rule.company-name", "users.rule.company-tax-id", "users.rule.customer-external-id", "users.rule.individual-email", "users.rule.individual-first-name", "users.rule.individual-last-name", "users.rule.individual-phone", Status = 400, Code = "validation_failed", Stage = "application")]
    [FlowOutcome("users.branch.create-customer.required-object", "A bound required individual/address object is explicitly null.", "Return 400 validation_failed with RequiredRequestObject before persistence.", "users.guard.required-request-object", Status = 400, Code = "validation_failed", Stage = "application")]
    internal sealed class CreateCustomerDescriptor : FlowDescriberBase<CreateCustomer>
    {
        [FlowStep(order: 1, bpmnId: "MapRequestToDomain", description: "Map bound input into a new Customer and company IDs. Omitted/null companies normalize to []; explicit null individual/addresses produce RequiredRequestObject / 400 validation_failed.", RuleId = "users.guard.required-request-object", RuleName = "RequiredRequestObject")]
        public Customer MapToDomain(CreateCustomerRequestDto request)
        {
            return MappingConfig.MapToDomain(request);
        }

        [FlowStep(order: 2, bpmnId: "ValidateCustomerAggregate", description: "Run CustomerValidationPolicy and collect its active rules/errors without writing; the next validation guard decides whether to return a safe failure.")]
        public async Task<ValidationResult> ValidateCustomer(Customer customer, IValidationPolicy<Customer> customerValidationPolicy)
        {
            return await customerValidationPolicy.Validate(customer);
        }

        [FlowStep(order: 3, bpmnId: "IsCustomerAggregateValid", description: "Continue when Customer validation passes; duplicate company tax IDs become 409 resource_conflict, while other errors become 400 validation_failed before persistence.")]
        public void ThrowValidationExceptionIfCustomerInvalid(Customer customer, ValidationResult validationResult)
        {
            if (validationResult.IsValid)
            {
                return;
            }

            if (validationResult.GetValidationErrors().Any(error => IsDuplicatedCompanyTaxIdError(error.Name)))
            {
                throw new ResourceAlreadyExistsException(
                    nameof(CreateCustomer),
                    GetDuplicatedCompanyTaxIds(customer),
                    nameof(CompanyData));
            }

            throw new ValidationException(validationResult);
        }

        [FlowStep(order: 4, bpmnId: "LoadExistingCustomerByExternalId", description: "Read the Customer by externalId; the following existence/conflict guard handles null or an existing match. Repository failures/cancellation stop the flow without this step writing.")]
        public async Task<Customer?> LoadCustomer(string externalId, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.GetByExternalIdAsync(externalId, cancellationToken);
        }

        [FlowStep(order: 5, bpmnId: "IsCustomerAlreadyExists", description: "Continue when the external account reference has no existing profile; otherwise return 409 resource_conflict before the write. The repository separately guards races.")]
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

        [FlowStep(order: 6, bpmnId: "SaveCustomer", description: "Atomically insert current and Insert history at version 0. Confirmed named external-ID uniqueness races map to 409 resource_conflict; Mongo write conflicts map to 409 concurrency_conflict. Failed transactions abort.")]
        public async Task<Customer> Save(Customer customer, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.CreateCustomer(customer, cancellationToken);
        }

        [FlowStep(order: 7, bpmnId: "MapCustomerResponse", description: "Map the current Customer into the complete response DTO; this mapping adds no database writes or history.")]
        public CustomerResponseDto MapToResponse(Customer customer)
        {
            return MappingConfig.MapToResponse(customer);
        }

        private static string GetDuplicatedCompanyTaxIds(Customer customer)
        {
            return string.Join(
                ",",
                customer.Companies
                    .Where(company => !string.IsNullOrWhiteSpace(company.TaxId))
                    .GroupBy(company => company.TaxId)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key));
        }

        private static bool IsDuplicatedCompanyTaxIdError(string errorName)
        {
            return errorName is "CompanyDataTaxIdUniquenessValidationRule" or "CustomerCompanyTaxIdValidationRule";
        }
    }
}


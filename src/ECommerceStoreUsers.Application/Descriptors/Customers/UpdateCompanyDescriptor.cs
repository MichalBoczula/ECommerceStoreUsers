using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
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
    internal sealed record UpdateCompany;

    [Documentation("users.flow.update-company", "Replace one company inside a Customer, enforce tax uniqueness and persist version/history; retain Customer UpdatedAt.")]
    [FlowOutcome("users.branch.update-company.saved", "Valid input passes checks and the repository commits the requested write.", "Return the complete 200 aggregate response with atomic current/history at the resulting version.", Status = 200, Stage = "application")]
    [FlowOutcome("users.branch.update-company.missing", "The aggregate is absent when initially loaded.", "Return 404 resource_not_found before saving.", Status = 404, Code = "resource_not_found", Stage = "application")]
    [FlowOutcome("users.branch.update-company.removed-before-save", "A competing write removes the loaded aggregate before persistence.", "Return 404 resource_not_found; preserve the competing deletion and all unrelated state.", Status = 404, Code = "resource_not_found", Stage = "persistence")]
    [FlowOutcome("users.branch.update-company.company-missing", "The Customer exists but the requested company ID is not in its companies.", "Return 404 resource_not_found before saving.", Status = 404, Code = "resource_not_found", Stage = "application")]
    [FlowOutcome("users.branch.update-company.duplicate-company", "Two companies within the same Customer would share the tax ID.", "Return 409 resource_conflict before saving; the translated conflict has no validation error array entries.", Status = 409, Code = "resource_conflict", Stage = "application")]
    [FlowOutcome("users.branch.update-company.write-conflict", "The repository detects a stale version or MongoDB write conflict 112.", "Abort the losing transaction and return 409 concurrency_conflict; retain the committed winner.", Status = 409, Code = "concurrency_conflict", Stage = "persistence")]
    [FlowOutcome("users.branch.update-company.validation", "An invoked policy collects errors and its validation guard rejects them.", "Return 400 validation_failed with complete rule errors and no writes.", "users.rule.address-apartment-number", "users.rule.address-building-number", "users.rule.address-city", "users.rule.address-postal-code", "users.rule.address-street", "users.rule.company-name", "users.rule.company-tax-id", "users.rule.empty-guid", Status = 400, Code = "validation_failed", Stage = "application")]
    [FlowOutcome("users.branch.update-company.required-object", "A bound required individual/address object is explicitly null.", "Return 400 validation_failed with RequiredRequestObject before persistence.", "users.guard.required-request-object", Status = 400, Code = "validation_failed", Stage = "application")]
    internal sealed class UpdateCompanyDescriptor : FlowDescriberBase<UpdateCompany>
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

        [FlowStep(order: 3, bpmnId: "ValidateCompanyId", description: "Run EmptyGuidValidationPolicy for CompanyId; collect errors for the next guard without checking resource existence.")]
        public async Task<ValidationResult> ValidateCompanyId(Guid companyId, IValidationPolicy<Guid> emptyGuidValidationPolicy)
        {
            return await emptyGuidValidationPolicy.Validate(companyId);
        }

        [FlowStep(order: 4, bpmnId: "IsCompanyIdValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfCompanyIdInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 5, bpmnId: "LoadCustomerProfile", description: "Read the Customer by resource UUID; the following existence/conflict guard handles null or an existing match. Repository failures/cancellation stop the flow without this step writing.")]
        public async Task<Customer?> LoadCustomer(Guid customerId, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.GetByIdAsync(customerId, cancellationToken);
        }

        [FlowStep(order: 6, bpmnId: "VerifyCustomerExists", description: "Continue when the requested Customer exists; otherwise throw 404 resource_not_found before persistence.")]
        public void ThrowNotFoundExceptionIfCustomerMissing(Guid customerId, Customer? customer)
        {
            if (customer is null)
            {
                throw new ResourceNotFoundException(nameof(UpdateCompany), customerId.ToString(), nameof(Customer));
            }
        }

        [FlowStep(order: 7, bpmnId: "MapAddress", description: "Map a complete address; the service invokes this once for billing and once for shipping. Explicit null becomes RequiredRequestObject / 400 validation_failed before any write.", RuleId = "users.guard.required-request-object", RuleName = "RequiredRequestObject")]
        public Address MapAddress(AddressRequestDto? address, string member)
        {
            return MappingConfig.MapAddress(address, member);
        }

        [FlowStep(order: 8, bpmnId: "LoadCompanyFromAggregate", description: "Find the requested company inside the loaded Customer; return null for the following 404 guard when it is absent. No repository write occurs.")]
        public CompanyData? LoadCompany(Customer customer, Guid companyId)
        {
            return customer.Companies.FirstOrDefault(x => x.Id == companyId);
        }

        [FlowStep(order: 9, bpmnId: "VerifyCompanyExists", description: "Continue when the requested company exists; otherwise throw 404 resource_not_found before persistence.")]
        public void ThrowNotFoundExceptionIfCompanyMissing(Guid companyId, CompanyData? company)
        {
            if (company is null)
            {
                throw new ResourceNotFoundException(nameof(LoadCompany), companyId.ToString(), nameof(CompanyData));
            }
        }

        [FlowStep(order: 10, bpmnId: "UpdateCompanyInsideAggregate", description: "Replace only the target company's values on the in-memory aggregate. The existing Customer UpdatedAt is retained; uniqueness and company rules run before saving.")]
        public void UpdateCompanyInsideAggregate(CompanyData company, UpdateCompanyRequestDto request, Address billing, Address shipping)
        {
            company.UpdateCompanyDetails(request.TaxId, request.CompanyName, billing, shipping);
        }


        [FlowStep(order: 11, bpmnId: "VerifyCompanyTaxIdIsUnique", description: "Continue unless another company in this Customer has the requested tax ID; an exact duplicate becomes 409 resource_conflict before saving.")]
        public void ThrowAlreadyExistsExceptionIfCompanyTaxIdExists(Customer customer, Guid companyId, string taxId)
        {
            if (customer.Companies.Any(company => company.Id != companyId && company.TaxId == taxId))
            {
                throw new ResourceAlreadyExistsException(
                    nameof(UpdateCompany),
                    taxId,
                    nameof(CompanyData));
            }
        }

        [FlowStep(order: 12, bpmnId: "ValidateCompanyData", description: "Run CompanyValidationPolicy and collect its active rules/errors without writing; the next validation guard decides whether to return a safe failure.")]
        public async Task<ValidationResult> ValidateCompany(CompanyData company, IValidationPolicy<CompanyData> companyValidationPolicy)
        {
            return await companyValidationPolicy.Validate(company);
        }

        [FlowStep(order: 13, bpmnId: "IsCompanyDataValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfCompanyInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 14, bpmnId: "SaveCustomer", description: "Atomically replace the Customer at version+1 and append Update history. A removed target maps to 404; stale versions or Mongo write conflict 112 map to 409 concurrency_conflict. Failed transactions abort without committing partial state.")]
        public async Task<Customer> Save(Customer customer, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.UpdateCustomer(customer, cancellationToken);
        }

        [FlowStep(order: 15, bpmnId: "MapCustomerResponse", description: "Map the current Customer into the complete response DTO; this mapping adds no database writes or history.")]
        public CustomerResponseDto MapToResponse(Customer customer)
        {
            return MappingConfig.MapToResponse(customer);
        }
    }
}


using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Admins
{
    internal sealed record GetAdminByExternalId;

    [Documentation("users.flow.get-admin-by-external-id", "Read an Admin by external identity without a login or write.")]
    [FlowOutcome("users.branch.get-admin-by-external-id.read", "The input is valid and the requested result is available.", "Return the complete read-only response with no stored changes.", Status = 200, Stage = "application")]
    [FlowOutcome("users.branch.get-admin-by-external-id.missing", "No profile exists for the external reference.", "Return 404 resource_not_found; leave all collections unchanged.", Status = 404, Code = "resource_not_found", Stage = "application")]
    [FlowOutcome("users.branch.get-admin-by-external-id.validation", "An invoked policy collects errors and its validation guard rejects them.", "Return 400 validation_failed with complete rule errors and no writes. GET external-ID validation uses synthetic valid non-key fields.", "users.rule.admin-external-id", Status = 400, Code = "validation_failed", Stage = "application")]
    internal sealed class GetAdminByExternalIdDescriptor : FlowDescriberBase<GetAdminByExternalId>
    {
        [FlowStep(order: 1, bpmnId: "MapExternalIdToAdminAggregate", description: "Build an in-memory Admin carrying externalId and valid synthetic name/email solely for validation; this is not a login or a persisted create.")]
        public Admin MapExternalIdToAdmin(string externalId)
        {
            return new Admin(externalId, "Validation Admin", "validation.admin@example.com");
        }

        [FlowStep(order: 2, bpmnId: "ValidateAdminExternalId", description: "Run AdminValidationPolicy and collect its active rules/errors without writing; the next validation guard decides whether to return a safe failure.")]
        public async Task<ValidationResult> ValidateAdmin(Admin admin, IValidationPolicy<Admin> adminValidationPolicy)
        {
            return await adminValidationPolicy.Validate(admin);
        }

        [FlowStep(order: 3, bpmnId: "IsAdminExternalIdValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfAdminInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 4, bpmnId: "LoadAdminProfile", description: "Read the Admin by externalId; the following existence/conflict guard handles null or an existing match. Repository failures/cancellation stop the flow without this step writing.")]
        public async Task<Admin?> LoadAdmin(string externalId, IAdminRepository adminRepository, CancellationToken cancellationToken)
        {
            return await adminRepository.GetByExternalIdAsync(externalId, cancellationToken);
        }

        [FlowStep(order: 5, bpmnId: "VerifyAdminExists", description: "Continue when the requested Admin exists; otherwise throw 404 resource_not_found before persistence.")]
        public void ThrowNotFoundExceptionIfAdminMissing(string externalId, Admin? admin)
        {
            if (admin is null)
            {
                throw new ResourceNotFoundException(
                    nameof(GetAdminByExternalId),
                    externalId,
                    nameof(Admin));
            }
        }

        [FlowStep(order: 6, bpmnId: "MapAdminResponse", description: "Map the current Admin into the complete response DTO; this mapping adds no database writes or history.")]
        public AdminResponseDto MapToResponse(Admin admin)
        {
            return MappingConfig.MapToResponse(admin);
        }
    }
}


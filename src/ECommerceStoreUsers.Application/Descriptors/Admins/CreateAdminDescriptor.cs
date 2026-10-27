using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Admins
{
    internal sealed record CreateAdmin;

    [Documentation("users.flow.create-admin", "Create an active Admin and atomic version-zero Insert history.")]
    [FlowOutcome("users.branch.create-admin.saved", "Valid input passes checks and the repository commits the requested write.", "Return the complete 200 aggregate response with atomic current/history at the resulting version.", Status = 200, Stage = "application")]
    [FlowOutcome("users.branch.create-admin.external-exists", "The external reference already resolves to a profile during the pre-insert read.", "Return 409 resource_conflict before creating current or history.", Status = 409, Code = "resource_conflict", Stage = "application")]
    [FlowOutcome("users.branch.create-admin.external-race", "A concurrent insert wins the named ExternalId unique key and its committed target is confirmed after abort.", "Return 409 resource_conflict and retain only the winning current/Insert history.", Status = 409, Code = "resource_conflict", Stage = "persistence")]
    [FlowOutcome("users.branch.create-admin.overlap", "Concurrent create requests overlap; the losing insert hits a confirmed ExternalId key or write conflict 112.", "Abort the losing transaction and return 409 resource_conflict for a confirmed unique-key collision or concurrency_conflict for a write conflict; retain the committed winner.", Status = 409, Code = "resource_conflict|concurrency_conflict", Stage = "persistence")]
    [FlowOutcome("users.branch.create-admin.validation", "An invoked policy collects errors and its validation guard rejects them.", "Return 400 validation_failed with complete rule errors and no writes.", "users.rule.admin-email", "users.rule.admin-external-id", "users.rule.admin-full-name", Status = 400, Code = "validation_failed", Stage = "application")]
    internal sealed class CreateAdminDescriptor : FlowDescriberBase<CreateAdmin>
    {
        [FlowStep(order: 1, bpmnId: "MapRequestToDomain", description: "Map the bound externalId/name/email into a new active Admin with generated ID and UTC LastLoginAt; nothing is saved yet.")]
        public Admin MapToDomain(CreateAdminRequestDto request)
        {
            return MappingConfig.MapToDomain(request);
        }

        [FlowStep(order: 2, bpmnId: "ValidateAdminAggregate", description: "Run AdminValidationPolicy and collect its active rules/errors without writing; the next validation guard decides whether to return a safe failure.")]
        public async Task<ValidationResult> ValidateAdmin(Admin admin, IValidationPolicy<Admin> adminValidationPolicy)
        {
            return await adminValidationPolicy.Validate(admin);
        }

        [FlowStep(order: 3, bpmnId: "IsAdminAggregateValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfAdminInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 4, bpmnId: "LoadExistingAdminByExternalId", description: "Read the Admin by externalId; the following existence/conflict guard handles null or an existing match. Repository failures/cancellation stop the flow without this step writing.")]
        public async Task<Admin?> LoadAdmin(string externalId, IAdminRepository adminRepository, CancellationToken cancellationToken)
        {
            return await adminRepository.GetByExternalIdAsync(externalId, cancellationToken);
        }

        [FlowStep(order: 5, bpmnId: "IsAdminAlreadyExists", description: "Continue when the external account reference has no existing profile; otherwise return 409 resource_conflict before the write. The repository separately guards races.")]
        public void ThrowAlreadyExistsExceptionIfAdminExists(string externalId, Admin? admin)
        {
            if (admin is not null)
            {
                throw new ResourceAlreadyExistsException(
                    nameof(CreateAdmin),
                    externalId,
                    nameof(Admin));
            }
        }

        [FlowStep(order: 6, bpmnId: "SaveAdmin", description: "Atomically insert current and Insert history at version 0. Confirmed named external-ID uniqueness races map to 409 resource_conflict; Mongo write conflicts map to 409 concurrency_conflict. Failed transactions abort.")]
        public async Task<Admin> Save(Admin admin, IAdminRepository adminRepository, CancellationToken cancellationToken)
        {
            return await adminRepository.CreateAdmin(admin, cancellationToken);
        }

        [FlowStep(order: 7, bpmnId: "MapAdminResponse", description: "Map the current Admin into the complete response DTO; this mapping adds no database writes or history.")]
        public AdminResponseDto MapToResponse(Admin admin)
        {
            return MappingConfig.MapToResponse(admin);
        }
    }
}

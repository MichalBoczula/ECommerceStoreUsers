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
    internal sealed record UpdateAdminProfile;

    [Documentation("users.flow.update-admin-profile", "Replace Admin name/email using optimistic versioning; unchanged values produce no history.")]
    [FlowOutcome("users.branch.update-admin-profile.saved", "The current version matches and the requested name/email differ from stored values.", "Return the complete 200 aggregate response with atomic current/history at the resulting version.", Status = 200, Stage = "application")]
    [FlowOutcome("users.branch.update-admin-profile.unchanged", "The current version and all candidate Admin profile values match stored data.", "Return 200 with unchanged current/version/history; no new Update snapshot.", Status = 200, Stage = "persistence")]
    [FlowOutcome("users.branch.update-admin-profile.missing", "The aggregate is absent when initially loaded.", "Return 404 resource_not_found before saving.", Status = 404, Code = "resource_not_found", Stage = "application")]
    [FlowOutcome("users.branch.update-admin-profile.removed-before-save", "A competing write removes the loaded aggregate before persistence.", "Return 404 resource_not_found; preserve the competing deletion and all unrelated state.", Status = 404, Code = "resource_not_found", Stage = "persistence")]
    [FlowOutcome("users.branch.update-admin-profile.write-conflict", "The repository detects a stale version or MongoDB write conflict 112.", "Abort the losing transaction and return 409 concurrency_conflict; retain the committed winner.", Status = 409, Code = "concurrency_conflict", Stage = "persistence")]
    [FlowOutcome("users.branch.update-admin-profile.validation", "An invoked policy collects errors and its validation guard rejects them.", "Return 400 validation_failed with complete rule errors and no writes.", "users.rule.admin-email", "users.rule.admin-external-id", "users.rule.admin-full-name", "users.rule.empty-guid", Status = 400, Code = "validation_failed", Stage = "application")]
    internal sealed class UpdateAdminProfileDescriptor : FlowDescriberBase<UpdateAdminProfile>
    {
        [FlowStep(order: 1, bpmnId: "ValidateAdminId", description: "Run EmptyGuidValidationPolicy for AdminId; collect errors for the next guard without checking resource existence.")]
        public async Task<ValidationResult> ValidateAdminId(Guid adminId, IValidationPolicy<Guid> emptyGuidValidationPolicy)
        {
            return await emptyGuidValidationPolicy.Validate(adminId);
        }

        [FlowStep(order: 2, bpmnId: "IsAdminIdValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfAdminIdInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 3, bpmnId: "LoadAdminProfile", description: "Read the Admin by resource UUID; the following existence/conflict guard handles null or an existing match. Repository failures/cancellation stop the flow without this step writing.")]
        public async Task<Admin?> LoadAdmin(Guid adminId, IAdminRepository adminRepository, CancellationToken cancellationToken)
        {
            return await adminRepository.GetByIdAsync(adminId, cancellationToken);
        }

        [FlowStep(order: 4, bpmnId: "VerifyAdminExists", description: "Continue when the requested Admin exists; otherwise throw 404 resource_not_found before persistence.")]
        public void ThrowNotFoundExceptionIfAdminMissing(Guid adminId, Admin? admin)
        {
            if (admin is null)
            {
                throw new ResourceNotFoundException(nameof(UpdateAdminProfile), adminId.ToString(), nameof(Admin));
            }
        }

        [FlowStep(order: 5, bpmnId: "MapRequestToAggregate", description: "Build the candidate Admin with requested name/email, retaining current ID, externalId, active flag, login timestamp and version; do not persist yet.")]
        public Admin MapRequestToAggregate(Admin admin, UpdateAdminProfileRequestDto request)
        {
            return Admin.Rehydrate(admin.Id, admin.ExternalId, request.FullName, request.Email, admin.IsActive, admin.LastLoginAt, admin.Version);
        }

        [FlowStep(order: 6, bpmnId: "ValidateAdminAggregate", description: "Run AdminValidationPolicy and collect its active rules/errors without writing; the next validation guard decides whether to return a safe failure.")]
        public async Task<ValidationResult> ValidateAdmin(Admin admin, IValidationPolicy<Admin> adminValidationPolicy)
        {
            return await adminValidationPolicy.Validate(admin);
        }

        [FlowStep(order: 7, bpmnId: "IsAdminAggregateValid", description: "Continue when the preceding validation result is valid; otherwise throw 400 validation_failed with its complete errors before any write.")]
        public void ThrowValidationExceptionIfAdminInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 8, bpmnId: "SaveAdmin", description: "After version/existence checks, unchanged profile values commit a no-op without a new version/history. Otherwise atomically replace at version+1 with Update history; removed targets give 404, stale/112 conflicts give 409, and failures abort.")]
        public async Task<Admin> Save(Admin admin, IAdminRepository adminRepository, CancellationToken cancellationToken)
        {
            return await adminRepository.UpdateAdmin(admin, cancellationToken);
        }

        [FlowStep(order: 9, bpmnId: "MapAdminResponse", description: "Map the current Admin into the complete response DTO; this mapping adds no database writes or history.")]
        public AdminResponseDto MapToResponse(Admin admin)
        {
            return MappingConfig.MapToResponse(admin);
        }
    }
}


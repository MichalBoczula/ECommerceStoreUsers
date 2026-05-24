using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Admins
{
    internal sealed record UpdateAdminProfile;

    internal sealed class UpdateAdminProfileDescriptor : FlowDescriberBase<UpdateAdminProfile>
    {
        [FlowStep(order: 1, bpmnId: "ValidateAdminId")]
        public async Task<ValidationResult> ValidateAdminId(Guid adminId, IValidationPolicy<Guid> emptyGuidValidationPolicy)
        {
            return await emptyGuidValidationPolicy.Validate(adminId);
        }

        [FlowStep(order: 2, bpmnId: "IsAdminIdValid")]
        public void ThrowValidationExceptionIfAdminIdInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 3, bpmnId: "LoadAdminProfile")]
        public async Task<Admin?> LoadAdmin(Guid adminId, IAdminRepository adminRepository, CancellationToken cancellationToken)
        {
            return await adminRepository.GetByIdAsync(adminId, cancellationToken);
        }

        [FlowStep(order: 4, bpmnId: "VerifyAdminExists")]
        public void ThrowNotFoundExceptionIfAdminMissing(Guid adminId, Admin? admin)
        {
            if (admin is null)
            {
                throw new ResourceNotFoundException(nameof(UpdateAdminProfile), adminId.ToString(), nameof(Admin));
            }
        }

        [FlowStep(order: 5, bpmnId: "MapRequestToAggregate")]
        public Admin MapRequestToAggregate(Admin admin, UpdateAdminProfileRequestDto request)
        {
            return Admin.Rehydrate(admin.Id, admin.ExternalId, request.FullName, request.Email, admin.IsActive, admin.LastLoginAt);
        }

        [FlowStep(order: 6, bpmnId: "ValidateAdminAggregate")]
        public async Task<ValidationResult> ValidateAdmin(Admin admin, IValidationPolicy<Admin> adminValidationPolicy)
        {
            return await adminValidationPolicy.Validate(admin);
        }

        [FlowStep(order: 7, bpmnId: "IsAdminAggregateValid")]
        public void ThrowValidationExceptionIfAdminInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 8, bpmnId: "SaveAdmin")]
        public async Task<Admin> Save(Admin admin, IAdminRepository adminRepository, CancellationToken cancellationToken)
        {
            return await adminRepository.UpdateAdmin(admin, cancellationToken);
        }

        [FlowStep(order: 9, bpmnId: "MapAdminResponse")]
        public AdminResponseDto MapToResponse(Admin admin)
        {
            return new AdminResponseDto
            {
                Id = admin.Id,
                ExternalId = admin.ExternalId,
                FullName = admin.FullName,
                Email = admin.Email,
                IsActive = admin.IsActive,
                LastLoginAt = admin.LastLoginAt
            };
        }
    }
}

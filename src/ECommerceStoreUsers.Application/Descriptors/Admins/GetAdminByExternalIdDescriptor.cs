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

    internal sealed class GetAdminByExternalIdDescriptor : FlowDescriberBase<GetAdminByExternalId>
    {
        [FlowStep(order: 1, bpmnId: "MapExternalIdToAdminAggregate")]
        public Admin MapExternalIdToAdmin(string externalId)
        {
            return new Admin(externalId, "Validation Admin", "validation.admin@example.com");
        }

        [FlowStep(order: 2, bpmnId: "ValidateAdminExternalId")]
        public async Task<ValidationResult> ValidateAdmin(Admin admin, IValidationPolicy<Admin> adminValidationPolicy)
        {
            return await adminValidationPolicy.Validate(admin);
        }

        [FlowStep(order: 3, bpmnId: "IsAdminExternalIdValid")]
        public void ThrowValidationExceptionIfAdminInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 4, bpmnId: "LoadAdminProfile")]
        public async Task<Admin?> LoadAdmin(string externalId, IAdminRepository adminRepository, CancellationToken cancellationToken)
        {
            return await adminRepository.GetByExternalIdAsync(externalId, cancellationToken);
        }

        [FlowStep(order: 5, bpmnId: "VerifyAdminExists")]
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

        [FlowStep(order: 6, bpmnId: "MapAdminResponse")]
        public AdminResponseDto MapToResponse(Admin admin)
        {
            return MappingConfig.MapToResponse(admin);
        }
    }
}

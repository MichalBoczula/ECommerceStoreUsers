using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Admins
{
    internal sealed record GetAdminByExternalId;

    internal sealed class GetAdminByExternalIdDescriptor : FlowDescriberBase<GetAdminByExternalId>
    {
        [FlowStep(order: 1, bpmnId: "LoadAdminProfile")]
        public async Task<Admin?> LoadAdmin(string externalId, IAdminRepository adminRepository, CancellationToken cancellationToken)
        {
            return await adminRepository.GetByExternalIdAsync(externalId, cancellationToken);
        }

        [FlowStep(order: 2, bpmnId: "VerifyAdminExists")]
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

        [FlowStep(order: 3, bpmnId: "MapAdminResponse")]
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

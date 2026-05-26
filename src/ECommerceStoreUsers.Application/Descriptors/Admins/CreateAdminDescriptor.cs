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

    internal sealed class CreateAdminDescriptor : FlowDescriberBase<CreateAdmin>
    {
        [FlowStep(order: 1, bpmnId: "MapRequestToDomain")]
        public Admin MapToDomain(CreateAdminRequestDto request)
        {
            return MappingConfig.MapToDomain(request);
        }

        [FlowStep(order: 2, bpmnId: "ValidateAdminAggregate")]
        public async Task<ValidationResult> ValidateAdmin(Admin admin, IValidationPolicy<Admin> adminValidationPolicy)
        {
            return await adminValidationPolicy.Validate(admin);
        }

        [FlowStep(order: 3, bpmnId: "IsAdminAggregateValid")]
        public void ThrowValidationExceptionIfAdminInvalid(ValidationResult validationResult)
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult);
            }
        }

        [FlowStep(order: 4, bpmnId: "LoadExistingAdminByExternalId")]
        public async Task<Admin?> LoadAdmin(string externalId, IAdminRepository adminRepository, CancellationToken cancellationToken)
        {
            return await adminRepository.GetByExternalIdAsync(externalId, cancellationToken);
        }

        [FlowStep(order: 5, bpmnId: "IsAdminAlreadyExists")]
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

        [FlowStep(order: 6, bpmnId: "SaveAdmin")]
        public async Task<Admin> Save(Admin admin, IAdminRepository adminRepository, CancellationToken cancellationToken)
        {
            return await adminRepository.CreateAdmin(admin, cancellationToken);
        }

        [FlowStep(order: 7, bpmnId: "MapAdminResponse")]
        public AdminResponseDto MapToResponse(Admin admin)
        {
            return MappingConfig.MapToResponse(admin);
        }
    }
}
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Application.Descriptors.Admins;
using ECommerceStoreUsers.Application.Services.Abstract.Admins;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using Microsoft.Extensions.Logging;

namespace ECommerceStoreUsers.Application.Services.Concrete.Admins
{
    internal class AdminProfileService(
        IAdminRepository _adminRepository,
        IValidationPolicy<Admin> _adminValidationPolicy,
        IValidationPolicy<Guid> _emptyGuidValidationPolicy,
        ILogger<AdminProfileService> _logger) : IAdminProfileService
    {
        public async Task<AdminResponseDto> GetAdminByExternalId(string externalId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating get admin by ExternalId flow for ExternalId: {ExternalId}", externalId);

            var descriptor = new GetAdminByExternalIdDescriptor();

            var admin = await descriptor.LoadAdmin(externalId, _adminRepository, cancellationToken);
            descriptor.ThrowNotFoundExceptionIfAdminMissing(externalId, admin);

            var response = descriptor.MapToResponse(admin!);

            _logger.LogInformation("Successfully loaded admin profile. AdminId: {AdminId} for ExternalId: {ExternalId}", response.Id, externalId);

            return response;
        }

        public async Task<AdminResponseDto> CreateAdmin(CreateAdminRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating admin creation flow for ExternalId: {ExternalId}", request.ExternalId);

            var descriptor = new CreateAdminDescriptor();

            var admin = descriptor.MapToDomain(request);

            var validationResult = await descriptor.ValidateAdmin(admin, _adminValidationPolicy);
            descriptor.ThrowValidationExceptionIfAdminInvalid(validationResult);

            var existingAdmin = await descriptor.LoadAdmin(admin.ExternalId, _adminRepository, cancellationToken);
            descriptor.ThrowAlreadyExistsExceptionIfAdminExists(admin.ExternalId, existingAdmin);

            var createdAdmin = await descriptor.Save(admin, _adminRepository, cancellationToken);

            var response = descriptor.MapToResponse(createdAdmin);

            _logger.LogInformation("Successfully created admin profile. AdminId: {AdminId} for ExternalId: {ExternalId}", response.Id, request.ExternalId);

            return response;
        }

        public async Task<AdminResponseDto> UpdateAdminProfile(Guid adminId, UpdateAdminProfileRequestDto request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initiating admin profile update flow for AdminId: {AdminId}", adminId);

            var descriptor = new UpdateAdminProfileDescriptor();

            var adminIdValidationResult = await descriptor.ValidateAdminId(adminId, _emptyGuidValidationPolicy);
            descriptor.ThrowValidationExceptionIfAdminIdInvalid(adminIdValidationResult);

            var admin = await descriptor.LoadAdmin(adminId, _adminRepository, cancellationToken);
            descriptor.ThrowNotFoundExceptionIfAdminMissing(adminId, admin);

            var updatedAdminModel = descriptor.MapRequestToAggregate(admin!, request);

            var validationResult = await descriptor.ValidateAdmin(updatedAdminModel, _adminValidationPolicy);
            descriptor.ThrowValidationExceptionIfAdminInvalid(validationResult);

            var updatedAdmin = await descriptor.Save(updatedAdminModel, _adminRepository, cancellationToken);

            var response = descriptor.MapToResponse(updatedAdmin);

            _logger.LogInformation("Successfully updated admin profile. AdminId: {AdminId}", adminId);

            return response;
        }
    }
}

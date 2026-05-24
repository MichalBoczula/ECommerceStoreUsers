using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Application.Descriptors.Admins;
using ECommerceStoreUsers.Application.Services.Abstract.Admins;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using Microsoft.Extensions.Logging;

namespace ECommerceStoreUsers.Application.Services.Concrete.Admins
{
    internal class AdminProfileService(
        IAdminRepository _adminRepository,
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

        public Task<AdminResponseDto> CreateAdmin(CreateAdminRequestDto request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<AdminResponseDto> UpdateAdminProfile(Guid adminId, UpdateAdminProfileRequestDto request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

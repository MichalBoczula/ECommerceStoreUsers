using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;

namespace ECommerceStoreUsers.Application.Services.Abstract.Admins
{
    public interface IAdminProfileService
    {
        Task<AdminResponseDto> GetAdminByExternalId(string externalId, CancellationToken cancellationToken);
        Task<AdminResponseDto> CreateAdmin(CreateAdminRequestDto request, CancellationToken cancellationToken);
        Task<AdminResponseDto> UpdateAdminProfile(Guid adminId, UpdateAdminProfileRequestDto request, CancellationToken cancellationToken);
    }
}

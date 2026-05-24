using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Application.Services.Abstract.Admins;

namespace ECommerceStoreUsers.Application.Services.Concrete.Admins
{
    internal class AdminProfileService : IAdminProfileService
    {
        public Task<AdminResponseDto> GetAdminByExternalId(string externalId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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

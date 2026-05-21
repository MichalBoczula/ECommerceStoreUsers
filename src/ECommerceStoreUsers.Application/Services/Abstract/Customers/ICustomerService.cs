using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;

namespace ECommerceStoreUsers.Application.Services.Abstract.Customers
{
    public interface ICustomerService
    {
        Task<CustomerResponseDto> GetCustomerByExternalId(string externalId, CancellationToken cancellationToken);
        Task<CustomerResponseDto> CreateCustomer(CreateCustomerRequestDto request, CancellationToken cancellationToken);
        Task<CustomerResponseDto> UpdateIndividualData(Guid id, UpdateIndividualDataRequestDto request, CancellationToken cancellationToken);
        Task<CustomerResponseDto> AddCompany(Guid externalId, AddCompanyRequestDto request, CancellationToken cancellationToken);
        Task<CustomerResponseDto> UpdateCompany(Guid externalId, Guid companyId, UpdateCompanyRequestDto request, CancellationToken cancellationToken);
    }
}
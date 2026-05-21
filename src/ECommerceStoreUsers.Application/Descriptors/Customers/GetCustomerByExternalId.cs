using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Descriptors.Customers
{
    internal sealed record GetCustomerByExternalId;

    internal sealed class GetCustomerByExternalIdDescriptor : FlowDescriberBase<GetCustomerByExternalId>
    {
        [FlowStep(order: 1, bpmnId: "LoadCustomerProfile")]
        public async Task<Customer?> LoadCustomer(string externalId, ICustomerRepository customerRepository, CancellationToken cancellationToken)
        {
            return await customerRepository.GetByExternalIdAsync(externalId, cancellationToken);
        }

        [FlowStep(order: 2, bpmnId: "VerifyCustomerExists")]
        public void ThrowNotFoundExceptionIfCustomerMissing(string externalId, Customer? customer)
        {
            if (customer is null)
            {
                throw new ResourceNotFoundException(
                    nameof(GetCustomerByExternalId),
                    externalId,
                    nameof(Customer));
            }
        }

        [FlowStep(order: 3, bpmnId: "MapCustomerResponse")]
        public CustomerResponseDto MapToResponse(Customer customer)
        {
            return MappingConfig.MapToResponse(customer);
        }
    }
}

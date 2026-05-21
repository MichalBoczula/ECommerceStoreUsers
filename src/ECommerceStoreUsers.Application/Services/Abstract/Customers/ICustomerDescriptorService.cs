using ECommerceStoreUsers.Application.Common.FlowDescriptors;

namespace ECommerceStoreUsers.Application.Services.Abstract.Customers
{
    public interface ICustomerDescriptorService
    {
        FlowDescriptor GetCreateCustomerDescriptor();
        FlowDescriptor GetCustomerByExternalIdDescriptor();
        FlowDescriptor GetUpdateIndividualDataDescriptor();
        FlowDescriptor GetAddCompanyDescriptor();
        FlowDescriptor GetUpdateCompanyDescriptor();
    }
}

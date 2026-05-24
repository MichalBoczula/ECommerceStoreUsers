using ECommerceStoreUsers.Application.Common.FlowDescriptors;

namespace ECommerceStoreUsers.Application.Services.Abstract.Admins
{
    public interface IAdminFlowDescriptorService
    {
        FlowDescriptor GetGetAdminByExternalIdDescriptor();
        FlowDescriptor GetCreateAdminDescriptor();
        FlowDescriptor GetUpdateAdminProfileDescriptor();
    }
}

using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Descriptors.Admins;
using ECommerceStoreUsers.Application.Services.Abstract.Admins;

namespace ECommerceStoreUsers.Application.Services.Concrete.Admins
{
    internal sealed class AdminFlowDescriptorService : IAdminFlowDescriptorService
    {
        public FlowDescriptor GetGetAdminByExternalIdDescriptor()
        {
            var descriptor = new GetAdminByExternalIdDescriptor();
            return descriptor.Describe();
        }

        public FlowDescriptor GetCreateAdminDescriptor()
        {
            throw new NotImplementedException();
        }

        public FlowDescriptor GetUpdateAdminProfileDescriptor()
        {
            throw new NotImplementedException();
        }
    }
}

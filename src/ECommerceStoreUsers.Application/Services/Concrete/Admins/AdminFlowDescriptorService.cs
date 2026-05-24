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
            var descriptor = new CreateAdminDescriptor();
            return descriptor.Describe();
        }

        public FlowDescriptor GetUpdateAdminProfileDescriptor()
        {
            var descriptor = new UpdateAdminProfileDescriptor();
            return descriptor.Describe();
        }
    }
}

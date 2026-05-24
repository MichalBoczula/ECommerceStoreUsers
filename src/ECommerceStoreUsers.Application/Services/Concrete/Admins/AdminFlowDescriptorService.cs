using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Services.Abstract.Admins;

namespace ECommerceStoreUsers.Application.Services.Concrete.Admins
{
    internal sealed class AdminFlowDescriptorService : IAdminFlowDescriptorService
    {
        public FlowDescriptor GetGetAdminByExternalIdDescriptor()
        {
            throw new NotImplementedException();
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

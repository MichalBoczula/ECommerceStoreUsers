using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Descriptors.Customers;
using ECommerceStoreUsers.Application.Services.Abstract.Customers;

namespace ECommerceStoreUsers.Application.Services.Concrete.Customers
{
    internal class CustomerDescriptorService : ICustomerDescriptorService
    {
        public FlowDescriptor GetCreateCustomerDescriptor()
        {
            var descriptor = new CreateCustomerDescriptor();
            return descriptor.Describe();
        }

        //public FlowDescriptor GetCustomerByExternalIdDescriptor()
        //{
        //    var descriptor = new GetCustomerByExternalIdDescriptor();
        //    return descriptor.Describe();
        //}

        //public FlowDescriptor GetUpdateIndividualDataDescriptor()
        //{
        //    var descriptor = new UpdateIndividualDataDescriptor();
        //    return descriptor.Describe();
        //}
    }
}

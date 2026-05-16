using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Abstract
{
    public interface IValidationPolicyDescriptorProvider
    {
        ValidationPolicyDescriptor Describe();
    }
}

using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Abstract
{
    public interface IValidationPolicy<T>
    {
        Task<ValidationResult> Validate(T entity);
        ValidationPolicyDescriptor Describe();
    }
}

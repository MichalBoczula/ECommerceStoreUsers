using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Abstract
{
    public interface IValidationRule<T>
    {
        Task IsValid(T entity, ValidationResult validationResults);
        List<ValidationError> Describe();
    }
}

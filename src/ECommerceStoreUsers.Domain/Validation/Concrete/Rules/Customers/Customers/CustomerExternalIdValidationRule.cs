using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Customers
{
    internal sealed class CustomerExternalIdValidationRule : IValidationRule<Customer>
    {
        private readonly ValidationError _invalidExternalId;

        public CustomerExternalIdValidationRule()
        {
            _invalidExternalId = new ValidationError
            {
                Message = "ExternalId cannot be null or white space.",
                Name = nameof(CustomerExternalIdValidationRule),
                Entity = nameof(Customer)
            };
        }

        public async Task IsValid(Customer entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            if (string.IsNullOrWhiteSpace(entity.ExternalId))
                validationResults.AddValidationError(_invalidExternalId);
        }

        public List<ValidationError> Describe() => [_invalidExternalId];
    }
}

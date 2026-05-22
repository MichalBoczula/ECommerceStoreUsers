using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Customers
{
    internal sealed class CustomerEmptyGuidValidationRule : IValidationRule<Customer>
    {
        private readonly ValidationError _emptyGuidClientId;
        private readonly ValidationError _emptyGuidExternalId;

        public CustomerEmptyGuidValidationRule()
        {
            _emptyGuidClientId = new ValidationError
            {
                Message = "ClientId cannot be an empty GUID.",
                Name = nameof(CustomerEmptyGuidValidationRule),
                Entity = nameof(Customer)
            };

            _emptyGuidExternalId = new ValidationError
            {
                Message = "ExternalId cannot be an empty GUID.",
                Name = nameof(CustomerEmptyGuidValidationRule),
                Entity = nameof(Customer)
            };
        }

        public async Task IsValid(Customer entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            if (entity.Id == Guid.Empty)
                validationResults.AddValidationError(_emptyGuidClientId);

            if (entity.ExternalId is null) return;

            if (Guid.TryParse(entity.ExternalId, out var externalId)
                && externalId == Guid.Empty)
                validationResults.AddValidationError(_emptyGuidExternalId);
        }

        public List<ValidationError> Describe() => [_emptyGuidClientId, _emptyGuidExternalId];
    }
}

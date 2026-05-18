using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Addresses
{
    internal sealed class AddressStreetValidationRule : IValidationRule<Address>
    {
        private readonly ValidationError _invalidStreet;

        public AddressStreetValidationRule()
        {
            _invalidStreet = new ValidationError
            {
                Message = "Street cannot be empty or white space.",
                Name = nameof(AddressStreetValidationRule),
                Entity = nameof(Address)
            };
        }

        public async Task IsValid(Address entity, ValidationResult validationResults)
        {
            if (string.IsNullOrWhiteSpace(entity.Street))
                validationResults.AddValidationError(_invalidStreet);
        }

        public List<ValidationError> Describe() => [_invalidStreet];
    }
}

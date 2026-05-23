using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.Addresses
{
    internal sealed class AddressPostalCodeValidationRule : IValidationRule<Address>
    {
        private readonly ValidationError _invalidPostalCode;

        public AddressPostalCodeValidationRule()
        {
            _invalidPostalCode = new ValidationError
            {
                Message = "Postal Code cannot be empty or white space.",
                Name = nameof(AddressPostalCodeValidationRule),
                Entity = nameof(Address)
            };
        }

        public async Task IsValid(Address entity, ValidationResult validationResults)
        {
            if (string.IsNullOrWhiteSpace(entity.PostalCode))
                validationResults.AddValidationError(_invalidPostalCode);
        }

        public List<ValidationError> Describe() => [_invalidPostalCode];
    }
}
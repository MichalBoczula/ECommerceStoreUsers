using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.Addresses
{
    internal sealed class AddressApartmentNumberValidationRule : IValidationRule<Address>
    {
        private readonly ValidationError _invalidApartmentNumber;

        public AddressApartmentNumberValidationRule()
        {
            _invalidApartmentNumber = new ValidationError
            {
                Message = "Apartment Number cannot be empty or white space.",
                Name = nameof(AddressApartmentNumberValidationRule),
                Entity = nameof(Address)
            };
        }

        public async Task IsValid(Address entity, ValidationResult validationResults)
        {
            if (entity.ApartmentNumber is null)
                return;

            if (entity.ApartmentNumber.Length == 0 || entity.ApartmentNumber.Trim().Length == 0)
            {
                validationResults.AddValidationError(_invalidApartmentNumber);
            }
        }

        public List<ValidationError> Describe() => [_invalidApartmentNumber];
    }
}

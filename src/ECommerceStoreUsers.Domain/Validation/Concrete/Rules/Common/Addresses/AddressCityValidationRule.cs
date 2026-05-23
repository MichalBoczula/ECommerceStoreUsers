using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.Addresses
{
    internal sealed class AddressCityValidationRule : IValidationRule<Address>
    {
        private readonly ValidationError _invalidCity;

        public AddressCityValidationRule()
        {
            _invalidCity = new ValidationError
            {
                Message = "City cannot be empty or white space.",
                Name = nameof(AddressCityValidationRule),
                Entity = nameof(Address)
            };
        }

        public async Task IsValid(Address entity, ValidationResult validationResults)
        {
            if (string.IsNullOrWhiteSpace(entity.City))
                validationResults.AddValidationError(_invalidCity);
        }

        public List<ValidationError> Describe() => [_invalidCity];
    }
}

using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Addresses
{
    internal sealed class AddressBuildingNumberValidationRule : IValidationRule<Address>
    {
        private readonly ValidationError _invalidBuildingNumber;

        public AddressBuildingNumberValidationRule()
        {
            _invalidBuildingNumber = new ValidationError
            {
                Message = "Building Number cannot be empty or white space.",
                Name = nameof(AddressBuildingNumberValidationRule),
                Entity = nameof(Address)
            };
        }

        public async Task IsValid(Address entity, ValidationResult validationResults)
        {
            if (string.IsNullOrWhiteSpace(entity.BuildingNumber))
                validationResults.AddValidationError(_invalidBuildingNumber);
        }

        public List<ValidationError> Describe() => [_invalidBuildingNumber];
    }
}

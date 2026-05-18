using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using System.Text.RegularExpressions;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Entities.IndividualData
{
    internal sealed class IndividualDataPhoneValidationRule : IValidationRule<IndividualData>
    {
        private readonly ValidationError _invalidPhone;
        private static readonly Regex PhoneRegex = new(@"^\d{7,10}$", RegexOptions.Compiled);

        public IndividualDataPhoneValidationRule()
        {
            _invalidPhone = new ValidationError
            {
                Message = "Phone must contain only digits and be between 7 and 10 characters long.",
                Name = nameof(IndividualDataPhoneValidationRule),
                Entity = nameof(IndividualData)
            };
        }

        public async Task IsValid(IndividualData entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            if (string.IsNullOrWhiteSpace(entity.Phone) || !PhoneRegex.IsMatch(entity.Phone))
                validationResults.AddValidationError(_invalidPhone);
        }

        public List<ValidationError> Describe() => [_invalidPhone];
    }
}

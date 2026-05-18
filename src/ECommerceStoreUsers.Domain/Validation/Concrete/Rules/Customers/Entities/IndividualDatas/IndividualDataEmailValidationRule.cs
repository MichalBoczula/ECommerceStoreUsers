using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using System.Text.RegularExpressions;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Entities.IndividualDatas
{
    internal sealed class IndividualDataEmailValidationRule : IValidationRule<IndividualData>
    {
        private readonly ValidationError _invalidEmail;
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public IndividualDataEmailValidationRule()
        {
            _invalidEmail = new ValidationError
            {
                Message = "Email must be a valid format (address@domain.something).",
                Name = nameof(IndividualDataEmailValidationRule),
                Entity = nameof(IndividualData)
            };
        }

        public async Task IsValid(IndividualData entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            if (string.IsNullOrWhiteSpace(entity.Email) || !EmailRegex.IsMatch(entity.Email))
                validationResults.AddValidationError(_invalidEmail);
        }

        public List<ValidationError> Describe() => [_invalidEmail];
    }
}

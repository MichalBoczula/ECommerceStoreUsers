using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using System.Text.RegularExpressions;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Entities.IndividualData
{
    internal sealed class IndividualDataFirstNameValidationRule : IValidationRule<IndividualData>
    {
        private readonly ValidationError _invalidFirstName;
        private static readonly Regex ContainsLetterRegex = new(@"\p{L}", RegexOptions.Compiled);

        public IndividualDataFirstNameValidationRule()
        {
            _invalidFirstName = new ValidationError
            {
                Message = "First Name cannot be empty or white space and must contain at least one letter.",
                Name = nameof(IndividualDataFirstNameValidationRule),
                Entity = nameof(IndividualData)
            };
        }

        public async Task IsValid(IndividualData entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            if (string.IsNullOrWhiteSpace(entity.FirstName) || !ContainsLetterRegex.IsMatch(entity.FirstName))
                validationResults.AddValidationError(_invalidFirstName);
        }

        public List<ValidationError> Describe() => [_invalidFirstName];
    }
}

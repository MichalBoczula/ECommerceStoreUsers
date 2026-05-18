using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using System.Text.RegularExpressions;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Entities.IndividualDatas
{
    internal sealed class IndividualDataLastNameValidationRule : IValidationRule<IndividualData>
    {
        private readonly ValidationError _invalidLastName;
        private static readonly Regex ContainsLetterRegex = new(@"\p{L}", RegexOptions.Compiled);

        public IndividualDataLastNameValidationRule()
        {
            _invalidLastName = new ValidationError
            {
                Message = "Last Name cannot be empty or white space and must contain at least one letter.",
                Name = nameof(IndividualDataLastNameValidationRule),
                Entity = nameof(IndividualData)
            };
        }

        public async Task IsValid(IndividualData entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            if (string.IsNullOrWhiteSpace(entity.LastName) || !ContainsLetterRegex.IsMatch(entity.LastName))
                validationResults.AddValidationError(_invalidLastName);
        }

        public List<ValidationError> Describe() => [_invalidLastName];
    }
}

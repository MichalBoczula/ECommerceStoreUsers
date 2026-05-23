using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using System.Text.RegularExpressions;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.CompanyDatas
{
    internal sealed class CompanyDataTaxIdValidationRule : IValidationRule<CompanyData>
    {
        private readonly ValidationError _invalidTaxId;
        private static readonly Regex PolishNipRegex = new(@"^\d{10}$", RegexOptions.Compiled);

        public CompanyDataTaxIdValidationRule()
        {
            _invalidTaxId = new ValidationError
            {
                Message = "Tax Id must be a valid Polish NIP containing exactly 10 digits.",
                Name = nameof(CompanyDataTaxIdValidationRule),
                Entity = nameof(CompanyData)
            };
        }

        public async Task IsValid(CompanyData entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            if (string.IsNullOrWhiteSpace(entity.TaxId) || !PolishNipRegex.IsMatch(entity.TaxId))
                validationResults.AddValidationError(_invalidTaxId);
        }

        public List<ValidationError> Describe() => [_invalidTaxId];
    }
}

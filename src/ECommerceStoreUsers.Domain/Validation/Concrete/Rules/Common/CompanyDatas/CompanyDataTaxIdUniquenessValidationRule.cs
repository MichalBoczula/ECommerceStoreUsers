using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.CompanyDatas
{
    internal sealed class CompanyDataTaxIdUniquenessValidationRule : IValidationRule<IReadOnlyCollection<CompanyData>>
    {
        private readonly ValidationError _duplicatedTaxId;

        public CompanyDataTaxIdUniquenessValidationRule()
        {
            _duplicatedTaxId = new ValidationError
            {
                Message = "Customer already contains a company with the same Tax Id.",
                Name = nameof(CompanyDataTaxIdUniquenessValidationRule),
                Entity = nameof(CompanyData)
            };
        }

        public async Task IsValid(IReadOnlyCollection<CompanyData> entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            var hasDuplicatedTaxId = entity
                .Where(company => !string.IsNullOrWhiteSpace(company.TaxId))
                .GroupBy(company => company.TaxId)
                .Any(group => group.Count() > 1);

            if (hasDuplicatedTaxId)
                validationResults.AddValidationError(_duplicatedTaxId);
        }

        public List<ValidationError> Describe() => [_duplicatedTaxId];
    }
}

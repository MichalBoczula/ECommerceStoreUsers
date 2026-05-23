using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common.CompanyDatas
{
    internal sealed class CompanyDataCompanyNameValidationRule : IValidationRule<CompanyData>
    {
        private readonly ValidationError _invalidCompanyName;

        public CompanyDataCompanyNameValidationRule()
        {
            _invalidCompanyName = new ValidationError
            {
                Message = "Company Name cannot be empty or white space.",
                Name = nameof(CompanyDataCompanyNameValidationRule),
                Entity = nameof(CompanyData)
            };
        }

        public async Task IsValid(CompanyData entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            if (string.IsNullOrWhiteSpace(entity.CompanyName))
                validationResults.AddValidationError(_invalidCompanyName);
        }

        public List<ValidationError> Describe() => [_invalidCompanyName];
    }
}

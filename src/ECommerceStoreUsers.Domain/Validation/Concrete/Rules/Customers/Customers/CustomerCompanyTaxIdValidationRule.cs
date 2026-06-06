using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Customers.Customers
{
    internal sealed class CustomerCompanyTaxIdValidationRule : IValidationRule<Customer>
    {
        private readonly ValidationError _duplicatedTaxId;

        public CustomerCompanyTaxIdValidationRule()
        {
            _duplicatedTaxId = new ValidationError
            {
                Message = "Customer already contains a company with the same Tax Id.",
                Name = nameof(CustomerCompanyTaxIdValidationRule),
                Entity = nameof(CompanyData)
            };
        }

        public async Task IsValid(Customer entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            var addedCompany = entity.Companies.LastOrDefault();
            if (addedCompany is null || string.IsNullOrWhiteSpace(addedCompany.TaxId)) return;

            var existingTaxIds = entity.Companies
                .Take(entity.Companies.Count - 1)
                .Select(company => company.TaxId);

            if (existingTaxIds.Contains(addedCompany.TaxId))
                validationResults.AddValidationError(_duplicatedTaxId);
        }

        public List<ValidationError> Describe() => [_duplicatedTaxId];
    }
}

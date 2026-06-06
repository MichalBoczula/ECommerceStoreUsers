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

            var hasDuplicatedTaxId = entity.Companies
                .Where(company => !string.IsNullOrWhiteSpace(company.TaxId))
                .GroupBy(company => company.TaxId)
                .Any(group => group.Count() > 1);

            if (hasDuplicatedTaxId)
                validationResults.AddValidationError(_duplicatedTaxId);
        }

        public List<ValidationError> Describe() => [_duplicatedTaxId];
    }
}

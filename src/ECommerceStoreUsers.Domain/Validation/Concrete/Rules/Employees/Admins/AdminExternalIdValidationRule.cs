using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Employees.Admins
{
    internal sealed class AdminExternalIdValidationRule : IValidationRule<Admin>
    {
        private readonly ValidationError _invalidExternalId;

        public AdminExternalIdValidationRule()
        {
            _invalidExternalId = new ValidationError
            {
                Message = "ExternalId cannot be null or white space.",
                Name = nameof(AdminExternalIdValidationRule),
                Entity = nameof(Admin)
            };
        }

        public async Task IsValid(Admin entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            if (string.IsNullOrWhiteSpace(entity.ExternalId))
                validationResults.AddValidationError(_invalidExternalId);
        }

        public List<ValidationError> Describe() => [_invalidExternalId];
    }
}
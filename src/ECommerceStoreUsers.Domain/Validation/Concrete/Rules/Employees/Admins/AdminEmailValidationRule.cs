using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using System.Text.RegularExpressions;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Employees.Admins
{
    internal sealed class AdminEmailValidationRule : IValidationRule<Admin>
    {
        private readonly ValidationError _invalidEmail;
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        public AdminEmailValidationRule()
        {
            _invalidEmail = new ValidationError
            {
                Message = "Email must be a valid format (address@domain.something).",
                Name = nameof(AdminEmailValidationRule),
                Entity = nameof(Admin)
            };
        }

        public async Task IsValid(Admin entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            if (string.IsNullOrWhiteSpace(entity.Email) || !EmailRegex.IsMatch(entity.Email))
                validationResults.AddValidationError(_invalidEmail);
        }

        public List<ValidationError> Describe() => [_invalidEmail];
    }
}
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using System.Text.RegularExpressions;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Employees.Admins
{
    internal sealed class AdminFullNameValidationRule : IValidationRule<Admin>
    {
        private readonly ValidationError _invalidFullName;
        private static readonly Regex SpecialCharsRegex = new(@"[@#$%^&*]", RegexOptions.Compiled);

        public AdminFullNameValidationRule()
        {
            _invalidFullName = new ValidationError
            {
                Message = "Full Name cannot be null, white space, or contain special characters (@#$%^&*).",
                Name = nameof(AdminFullNameValidationRule),
                Entity = nameof(Admin)
            };
        }

        public async Task IsValid(Admin entity, ValidationResult validationResults)
        {
            if (entity is null) return;

            if (string.IsNullOrWhiteSpace(entity.FullName) || SpecialCharsRegex.IsMatch(entity.FullName))
                validationResults.AddValidationError(_invalidFullName);
        }

        public List<ValidationError> Describe() => [_invalidFullName];
    }
}
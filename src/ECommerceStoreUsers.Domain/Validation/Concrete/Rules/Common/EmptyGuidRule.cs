using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common;

internal sealed class EmptyGuidRule : IValidationRule<Guid>
{
    private readonly ValidationError _emptyGuidError;

    public EmptyGuidRule()
    {
        _emptyGuidError = new ValidationError
        {
            Message = "Guid cannot be empty.",
            Name = nameof(EmptyGuidRule),
            Entity = nameof(Guid)
        };
    }

    public async Task IsValid(Guid entity, ValidationResult validationResults)
    {
        if (entity == Guid.Empty)
            validationResults.AddValidationError(_emptyGuidError);
    }

    public List<ValidationError> Describe() => [_emptyGuidError];
}

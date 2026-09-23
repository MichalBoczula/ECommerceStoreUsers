namespace ECommerceStoreUsers.Domain.Validation.Common;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException() : base("The customer was changed by another request.") { }
}

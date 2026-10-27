namespace ECommerceStoreUsers.Domain.Validation.Common
{
    /// <summary>Complete business validation error signature.</summary>
    public sealed record ValidationError
    {
        /// <summary>Human-readable validation error message.</summary>
        /// <example>ExternalId cannot be null or white space.</example>
        public required string Message { get; init; }
        /// <summary>Validation rule name associated with this error.</summary>
        /// <example>AdminExternalIdValidationRule</example>
        public required string Name { get; init; }
        /// <summary>Domain entity name associated with this error.</summary>
        /// <example>Admin</example>
        public required string Entity { get; init; }
    }
}


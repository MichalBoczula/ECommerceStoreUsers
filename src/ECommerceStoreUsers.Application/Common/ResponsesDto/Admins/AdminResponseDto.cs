namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Admins
{
    /// <summary>Complete current administrative account profile.</summary>
    public sealed record AdminResponseDto
    {
        /// <summary>Server-generated resource identifier.</summary>
        /// <example>11111111-1111-1111-1111-111111111111</example>
        public required Guid Id { get; init; }
        /// <summary>External account reference. Required and nonblank; unique within the Customer or Admin collection.</summary>
        /// <example>account-123</example>
        public required string ExternalId { get; init; }
        /// <summary>Admin display name. Required and nonblank; the active policy rejects its configured special characters.</summary>
        /// <example>Anna Kowalska</example>
        public required string FullName { get; init; }
        /// <summary>Email text checked by the active business email rule. This is not a claim of full RFC email validation.</summary>
        /// <example>anna@example.com</example>
        public required string Email { get; init; }
        /// <summary>Current administrative account activation flag.</summary>
        /// <example>true</example>
        public required bool IsActive { get; init; }
        /// <summary>UTC timestamp initialized on Admin creation; this API does not update it as a login operation.</summary>
        /// <example>2026-10-27T10:00:00Z</example>
        public required DateTime LastLoginAt { get; init; }
    }
}


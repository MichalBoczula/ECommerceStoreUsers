using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Admins
{
    /// <summary>Admin creation input. Required members must be present; explicit null strings reach business validation. JSON member matching is case-insensitive and unknown members are ignored.</summary>
    public sealed record CreateAdminRequestDto
    {
        /// <summary>External account reference. Required and nonblank; unique within the Customer or Admin collection.</summary>
        /// <example>account-123</example>
        [property: JsonRequired] public required string ExternalId { get; init; }
        /// <summary>Admin display name. Required and nonblank; the active policy rejects its configured special characters.</summary>
        /// <example>Anna Kowalska</example>
        [property: JsonRequired] public required string FullName { get; init; }
        /// <summary>Email text checked by the active business email rule. This is not a claim of full RFC email validation.</summary>
        /// <example>anna@example.com</example>
        [property: JsonRequired] public required string Email { get; init; }
    }
}


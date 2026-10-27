using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Admins
{
    /// <summary>Complete Admin profile replacement input. Unknown members are ignored; names match case-insensitively.</summary>
    public sealed record UpdateAdminProfileRequestDto
    {
        /// <summary>Admin display name. Required and nonblank; the active policy rejects its configured special characters.</summary>
        /// <example>Anna Kowalska</example>
        [property: JsonRequired] public required string FullName { get; init; }
        /// <summary>Email text checked by the active business email rule. This is not a claim of full RFC email validation.</summary>
        /// <example>anna@example.com</example>
        [property: JsonRequired] public required string Email { get; init; }
    }
}


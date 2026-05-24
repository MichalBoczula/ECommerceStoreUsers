using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Admins
{
    public sealed record CreateAdminRequestDto
    {
        [property: JsonRequired] public required string ExternalId { get; init; }
        [property: JsonRequired] public required string FullName { get; init; }
        [property: JsonRequired] public required string Email { get; init; }
    }
}

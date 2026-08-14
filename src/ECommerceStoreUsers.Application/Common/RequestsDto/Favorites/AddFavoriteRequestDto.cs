using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Favorites
{
    public sealed record AddFavoriteRequestDto
    {
        [property: JsonRequired] public required Guid ProductId { get; init; }
    }
}

using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Favorites
{
    /// <summary>Favorite input with a required UUID productId. Unknown members are ignored; names match case-insensitively.</summary>
    public sealed record AddFavoriteRequestDto
    {
        /// <summary>Product identifier; Users checks that it is not empty but does not query the catalog.</summary>
        /// <example>33333333-3333-3333-3333-333333333333</example>
        [property: JsonRequired] public required Guid ProductId { get; init; }
    }
}


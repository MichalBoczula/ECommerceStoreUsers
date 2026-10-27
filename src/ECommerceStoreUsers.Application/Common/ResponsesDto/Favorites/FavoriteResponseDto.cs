namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Favorites
{
    /// <summary>Saved client/product pair and its UTC creation timestamp.</summary>
    public sealed record FavoriteResponseDto
    {
        /// <summary>Server-generated resource identifier.</summary>
        /// <example>11111111-1111-1111-1111-111111111111</example>
        public required Guid Id { get; init; }
        /// <summary>Client identifier supplied in the route; Users does not check that the client exists.</summary>
        /// <example>22222222-2222-2222-2222-222222222222</example>
        public required Guid ClientId { get; init; }
        /// <summary>Product identifier; Users checks that it is not empty but does not query the catalog.</summary>
        /// <example>33333333-3333-3333-3333-333333333333</example>
        public required Guid ProductId { get; init; }
        /// <summary>UTC timestamp when this Favorite was added.</summary>
        /// <example>2026-10-27T10:00:00Z</example>
        public required DateTime AddedAt { get; init; }
    }
}


namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Favorites
{
    public sealed record FavoriteResponseDto
    {
        public required Guid Id { get; init; }
        public required Guid ClientId { get; init; }
        public required Guid ProductId { get; init; }
        public required DateTime AddedAt { get; init; }
    }
}

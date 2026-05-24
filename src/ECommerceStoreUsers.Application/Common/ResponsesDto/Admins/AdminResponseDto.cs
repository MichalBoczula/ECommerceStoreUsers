namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Admins
{
    public sealed record AdminResponseDto
    {
        public required Guid Id { get; init; }
        public required string ExternalId { get; init; }
        public required string FullName { get; init; }
        public required string Email { get; init; }
        public required bool IsActive { get; init; }
        public required DateTime LastLoginAt { get; init; }
    }
}

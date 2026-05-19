namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Customers
{
    public sealed record CustomerResponseDto
    {
        public required Guid Id { get; init; }
        public required string ExternalId { get; init; }
        public required IndividualDataResponseDto Individual { get; init; }
        public required IReadOnlyCollection<CompanyDataResponseDto> Companies { get; init; }
        public required DateTime UpdatedAt { get; init; }
    }
}

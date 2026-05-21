namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Customers
{
    public sealed record AddressResponseDto
    {
        public required string PostalCode { get; init; }
        public required string City { get; init; }
        public required string Street { get; init; }
        public required string BuildingNumber { get; init; }
        public required string? ApartmentNumber { get; init; }
    }
}

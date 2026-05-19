namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Customers
{
    public sealed record CompanyDataResponseDto
    {
        public required Guid Id { get; init; }
        public required string TaxId { get; init; }
        public required string CompanyName { get; init; }
        public required AddressResponseDto BillingAddress { get; init; }
        public required AddressResponseDto ShippingAddress { get; init; }
    }
}

namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Customers
{
    public sealed record IndividualDataResponseDto
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Email { get; init; }
        public required string Phone { get; init; }
        public required AddressResponseDto BillingAddress { get; init; }
        public required AddressResponseDto ShippingAddress { get; init; }
    }
}

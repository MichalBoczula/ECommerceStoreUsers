namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    public sealed record IndividualDataRequestDto
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Email { get; init; }
        public required string Phone { get; init; }
        public required AddressRequestDto BillingAddress { get; init; }
        public required AddressRequestDto ShippingAddress { get; init; }
    }
}

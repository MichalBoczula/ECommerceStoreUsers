namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    public sealed record UpdateCompanyRequestDto
    {
        public required string TaxId { get; init; }
        public required string CompanyName { get; init; }
        public required AddressRequestDto BillingAddress { get; init; }
        public required AddressRequestDto ShippingAddress { get; init; }
    }
}

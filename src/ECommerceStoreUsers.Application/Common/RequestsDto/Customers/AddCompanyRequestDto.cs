using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    public sealed record AddCompanyRequestDto
    {
        [property: JsonRequired] public required string TaxId { get; init; }
        [property: JsonRequired] public required string CompanyName { get; init; }
        [property: JsonRequired] public required AddressRequestDto BillingAddress { get; init; }
        [property: JsonRequired] public required AddressRequestDto ShippingAddress { get; init; }
    }
}

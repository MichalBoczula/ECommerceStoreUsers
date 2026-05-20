using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    public sealed record IndividualDataRequestDto
    {
        [property: JsonRequired] public required string FirstName { get; init; }
        [property: JsonRequired] public required string LastName { get; init; }
        [property: JsonRequired] public required string Email { get; init; }
        [property: JsonRequired] public required string Phone { get; init; }
        [property: JsonRequired] public required AddressRequestDto BillingAddress { get; init; }
        [property: JsonRequired] public required AddressRequestDto ShippingAddress { get; init; }
    }
}

using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    public sealed record AddressRequestDto
    {
        [property: JsonRequired] public required string PostalCode { get; init; }
        [property: JsonRequired] public required string City { get; init; }
        [property: JsonRequired] public required string Street { get; init; }
        [property: JsonRequired] public required string BuildingNumber { get; init; }
        public required string? ApartmentNumber { get; init; }
    }
}

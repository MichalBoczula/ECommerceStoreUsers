using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    public sealed record CreateCustomerRequestDto
    {
        [property: JsonRequired] public required string ExternalId { get; init; }
        [property: JsonRequired] public required IndividualDataRequestDto Individual { get; init; }
        public IReadOnlyCollection<AddCompanyRequestDto> Companies { get; init; } = [];
    }
}

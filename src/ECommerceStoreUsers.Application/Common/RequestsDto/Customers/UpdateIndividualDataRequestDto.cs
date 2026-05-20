using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    public sealed record UpdateIndividualDataRequestDto
    {
        [property: JsonRequired] public required IndividualDataRequestDto Individual { get; init; }
    }
}

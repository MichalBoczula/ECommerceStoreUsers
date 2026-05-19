namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    public sealed record UpdateIndividualDataRequestDto
    {
        public required IndividualDataRequestDto Individual { get; init; }
    }
}

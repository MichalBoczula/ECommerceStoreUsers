namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    public sealed record CreateCustomerRequestDto
    {
        public required string ExternalId { get; init; }
        public required IndividualDataRequestDto Individual { get; init; }
    }
}

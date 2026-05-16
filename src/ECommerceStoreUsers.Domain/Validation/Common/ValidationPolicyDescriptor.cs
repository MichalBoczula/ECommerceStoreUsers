namespace ECommerceStoreUsers.Domain.Validation.Common
{
    public sealed record ValidationPolicyDescriptor
    {
        public required string PolicyName { get; init; }
        public required List<ValidationRuleDescriptor> Rules { get; init; }
    }
}

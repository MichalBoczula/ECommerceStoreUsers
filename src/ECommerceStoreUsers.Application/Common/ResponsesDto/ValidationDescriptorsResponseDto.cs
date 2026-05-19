using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Common.ResponsesDto
{
    public sealed record ValidationDescriptorsResponseDto
    {
        public required List<Dictionary<string, ValidationPolicyDescriptor>> Validations { get; init; }
    }
}

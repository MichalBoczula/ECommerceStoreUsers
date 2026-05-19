using ECommerceStoreUsers.Application.Common.FlowDescriptors;

namespace ECommerceStoreUsers.Application.Common.ResponsesDto
{
    public sealed record FlowDescriptorsResponseDto
    {
        public required List<Dictionary<string, FlowDescriptor>> Flows { get; init; }
    }
}
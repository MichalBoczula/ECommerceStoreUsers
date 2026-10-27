using ECommerceStoreUsers.Application.Common.FlowDescriptors;

namespace ECommerceStoreUsers.Application.Common.ResponsesDto
{
    /// <summary>Published application flow descriptors, keyed by descriptor name.</summary>
    public sealed record FlowDescriptorsResponseDto
    {
        /// <summary>Array of descriptor-name dictionaries containing application flow descriptions.</summary>
        /// <example>[{&quot;GetCreateAdminDescriptor&quot;:{&quot;flowName&quot;:&quot;CreateAdmin&quot;,&quot;steps&quot;:[{&quot;order&quot;:1,&quot;stepName&quot;:&quot;Receive request&quot;,&quot;bpmnId&quot;:&quot;example-step&quot;}]}}]</example>
        public required List<Dictionary<string, FlowDescriptor>> Flows { get; init; }
    }
}

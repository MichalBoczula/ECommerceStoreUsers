namespace ECommerceStoreUsers.Application.Common.FlowDescriptors
{
    /// <summary>Application flow name and ordered step descriptions.</summary>
    public sealed record FlowDescriptor
    {
        /// <summary>Stable name of the application flow.</summary>
        /// <example>CreateAdmin</example>
        public required string FlowName { get; init; }
        /// <summary>Ordered flow steps. Their meaning is supplied by the application descriptor.</summary>
        /// <example>[{&quot;order&quot;:1,&quot;stepName&quot;:&quot;Receive request&quot;,&quot;bpmnId&quot;:&quot;example-step&quot;}]</example>
        public required List<FlowStepDescriptor> Steps { get; init; }
    }
}


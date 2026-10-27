namespace ECommerceStoreUsers.Application.Common.FlowDescriptors
{
    /// <summary>One identified and ordered application flow step.</summary>
    public sealed record FlowStepDescriptor
    {
        /// <summary>One-based order of this step within its flow.</summary>
        /// <example>1</example>
        public required int Order { get; init; }
        /// <summary>Human-readable application step description.</summary>
        /// <example>Receive request</example>
        public required string StepName { get; init; }
        /// <summary>Descriptor-supplied identifier linking this step to its flow diagram.</summary>
        /// <example>example-step</example>
        public required string BpmnId { get; init; }
    }
}


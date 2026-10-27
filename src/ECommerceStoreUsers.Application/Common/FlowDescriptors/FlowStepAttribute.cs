namespace ECommerceStoreUsers.Application.Common.FlowDescriptors
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class FlowStepAttribute(int order, string bpmnId, string description) : Attribute
    {
        public int Order { get; } = order;
        public string BpmnId { get; } = bpmnId;
        public string Description { get; } = description;
        public string? RuleId { get; set; }
        public string? RuleName { get; set; }
    }
}


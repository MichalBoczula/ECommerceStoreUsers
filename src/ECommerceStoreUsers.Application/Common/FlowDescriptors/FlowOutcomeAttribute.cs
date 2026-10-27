namespace ECommerceStoreUsers.Application.Common.FlowDescriptors;

/// <summary>Declared outcome semantics, joined to scenario intent rather than presented as execution telemetry.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class FlowOutcomeAttribute(string id, string condition, string effects, params string[] ruleIds) : Attribute
{
    public string Id { get; } = id;
    public string Condition { get; } = condition;
    public string Effects { get; } = effects;
    public string[] RuleIds { get; } = ruleIds;
    public int Status { get; set; }
    public string? Code { get; set; }
    public string Stage { get; set; } = "application";
}

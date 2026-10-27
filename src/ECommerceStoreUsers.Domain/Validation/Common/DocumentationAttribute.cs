namespace ECommerceStoreUsers.Domain.Validation.Common;

/// <summary>Stable identity and authored meaning for a flow, policy or rule; does not execute validation.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DocumentationAttribute(string id, string summary) : Attribute
{
    public string Id { get; } = id;
    public string Summary { get; } = summary;
}

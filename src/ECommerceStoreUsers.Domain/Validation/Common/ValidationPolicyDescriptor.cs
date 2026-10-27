namespace ECommerceStoreUsers.Domain.Validation.Common
{
    /// <summary>One validation policy and its described rules.</summary>
    public sealed record ValidationPolicyDescriptor
    {
        /// <summary>Name of the validation policy being described.</summary>
        /// <example>AdminValidationPolicy</example>
        public required string PolicyName { get; init; }
        /// <summary>Described rule entries and their validation errors.</summary>
        /// <example>[{&quot;ruleName&quot;:&quot;AdminExternalIdValidationRule&quot;,&quot;rules&quot;:[{&quot;message&quot;:&quot;ExternalId cannot be null or white space.&quot;,&quot;name&quot;:&quot;AdminExternalIdValidationRule&quot;,&quot;entity&quot;:&quot;Admin&quot;}]}]</example>
        public required List<ValidationRuleDescriptor> Rules { get; init; }
    }
}


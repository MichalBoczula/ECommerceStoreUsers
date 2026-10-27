namespace ECommerceStoreUsers.Domain.Validation.Common
{
    /// <summary>One validation rule and the complete errors that it describes.</summary>
    public sealed record ValidationRuleDescriptor
    {
        /// <summary>Name of the described validation rule.</summary>
        /// <example>AdminExternalIdValidationRule</example>
        public required string RuleName { get; init; }
        /// <summary>Complete error signatures associated with this rule.</summary>
        /// <example>[{&quot;message&quot;:&quot;ExternalId cannot be null or white space.&quot;,&quot;name&quot;:&quot;AdminExternalIdValidationRule&quot;,&quot;entity&quot;:&quot;Admin&quot;}]</example>
        public required List<ValidationError> Rules { get; init; }
    }
}


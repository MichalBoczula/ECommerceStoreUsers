using ECommerceStoreUsers.Domain.Validation.Common;

namespace ECommerceStoreUsers.Application.Common.ResponsesDto
{
    /// <summary>Published validation policy descriptors, keyed by policy name.</summary>
    public sealed record ValidationDescriptorsResponseDto
    {
        /// <summary>Array of policy-name dictionaries containing validation rule descriptions.</summary>
        /// <example>[{&quot;AdminValidationPolicy&quot;:{&quot;policyName&quot;:&quot;AdminValidationPolicy&quot;,&quot;rules&quot;:[{&quot;ruleName&quot;:&quot;AdminExternalIdValidationRule&quot;,&quot;rules&quot;:[{&quot;message&quot;:&quot;ExternalId cannot be null or white space.&quot;,&quot;name&quot;:&quot;AdminExternalIdValidationRule&quot;,&quot;entity&quot;:&quot;Admin&quot;}]}]}}]</example>
        public required List<Dictionary<string, ValidationPolicyDescriptor>> Validations { get; init; }
    }
}


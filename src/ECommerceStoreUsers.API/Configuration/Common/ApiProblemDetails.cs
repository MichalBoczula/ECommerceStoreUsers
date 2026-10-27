using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceStoreUsers.API.Configuration.Common
{
    /// <summary>Safe API failure. The writer always emits all nine fields; errors and missingProperties are arrays, including when empty. Problem extensions remain permitted.</summary>
    public sealed class ApiProblemDetails : ProblemDetails
    {
        /// <summary>Stable machine-readable failure code.</summary>
        /// <example>validation_failed</example>
        public string Code { get; init; } = string.Empty;
        /// <summary>Complete validation error entries; empty for failures without business-rule errors.</summary>
        /// <example>[{&quot;message&quot;:&quot;ExternalId cannot be null or white space.&quot;,&quot;name&quot;:&quot;AdminExternalIdValidationRule&quot;,&quot;entity&quot;:&quot;Admin&quot;}]</example>
        public IEnumerable<ValidationError> Errors { get; init; } = [];
        /// <summary>Names of missing required JSON members identified by binding; empty for other failures.</summary>
        /// <example>[]</example>
        public IReadOnlyCollection<string> MissingProperties { get; init; } = [];
        /// <summary>Request trace identifier for correlation; no exception internals.</summary>
        /// <example>trace-example-001</example>
        public string TraceId { get; init; } = string.Empty;
    }
}


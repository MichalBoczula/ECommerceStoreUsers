using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceStoreUsers.API.Configuration.Common
{
    public sealed class ApiProblemDetails : ProblemDetails
    {
        public string Code { get; init; } = string.Empty;
        public IEnumerable<ValidationError> Errors { get; init; } = [];
        public IReadOnlyCollection<string> MissingProperties { get; init; } = [];
        public string TraceId { get; init; } = string.Empty;
    }
}

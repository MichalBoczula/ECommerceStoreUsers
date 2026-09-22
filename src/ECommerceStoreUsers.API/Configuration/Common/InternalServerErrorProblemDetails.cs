using Microsoft.AspNetCore.Mvc;

namespace ECommerceStoreUsers.API.Configuration.Common
{
    public sealed class InternalServerErrorProblemDetails : ProblemDetails
    {
        public string TraceId { get; init; } = string.Empty;
    }
}

using Microsoft.AspNetCore.Mvc;

namespace ECommerceStoreInvoice.API.Configuration.Common
{
    public sealed class InternalServerErrorProblemDetails : ProblemDetails
    {
        public string TraceId { get; init; } = string.Empty;
    }
}

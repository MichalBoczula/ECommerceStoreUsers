namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Customers
{
    /// <summary>Complete current company profile with a server-generated identifier.</summary>
    public sealed record CompanyDataResponseDto
    {
        /// <summary>Server-generated resource identifier.</summary>
        /// <example>11111111-1111-1111-1111-111111111111</example>
        public required Guid Id { get; init; }
        /// <summary>Company tax identifier checked by the active business rule and unique within one Customer aggregate.</summary>
        /// <example>1234567890</example>
        public required string TaxId { get; init; }
        /// <summary>Company name checked by the active business rule.</summary>
        /// <example>Example Company</example>
        public required string CompanyName { get; init; }
        /// <summary>Complete current billing address.</summary>
        /// <example>{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}</example>
        public required AddressResponseDto BillingAddress { get; init; }
        /// <summary>Complete current shipping address.</summary>
        /// <example>{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}</example>
        public required AddressResponseDto ShippingAddress { get; init; }
    }
}


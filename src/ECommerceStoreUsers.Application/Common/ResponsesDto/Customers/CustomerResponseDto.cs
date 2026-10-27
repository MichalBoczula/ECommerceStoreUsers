namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Customers
{
    /// <summary>Complete Customer profile with current individual/company values and UTC update timestamp.</summary>
    public sealed record CustomerResponseDto
    {
        /// <summary>Server-generated resource identifier.</summary>
        /// <example>11111111-1111-1111-1111-111111111111</example>
        public required Guid Id { get; init; }
        /// <summary>External account reference. Required and nonblank; unique within the Customer or Admin collection.</summary>
        /// <example>account-123</example>
        public required string ExternalId { get; init; }
        /// <summary>Complete current individual profile.</summary>
        /// <example>{&quot;firstName&quot;:&quot;Anna&quot;,&quot;lastName&quot;:&quot;Kowalska&quot;,&quot;email&quot;:&quot;anna@example.com&quot;,&quot;phone&quot;:&quot;123456789&quot;,&quot;billingAddress&quot;:{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null},&quot;shippingAddress&quot;:{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}}</example>
        public required IndividualDataResponseDto Individual { get; init; }
        /// <summary>Complete company profiles; an empty collection is emitted as [], never null.</summary>
        /// <example>[{&quot;id&quot;:&quot;11111111-1111-1111-1111-111111111111&quot;,&quot;taxId&quot;:&quot;1234567890&quot;,&quot;companyName&quot;:&quot;Example Company&quot;,&quot;billingAddress&quot;:{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null},&quot;shippingAddress&quot;:{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}}]</example>
        public required IReadOnlyCollection<CompanyDataResponseDto> Companies { get; init; }
        /// <summary>UTC timestamp of the Customer aggregate update.</summary>
        /// <example>2026-10-27T10:00:00Z</example>
        public required DateTime UpdatedAt { get; init; }
    }
}


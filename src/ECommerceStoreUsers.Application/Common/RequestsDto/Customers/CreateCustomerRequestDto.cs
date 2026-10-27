using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    /// <summary>Customer creation input. companies may be omitted or null and is treated as an empty collection. JSON member matching is case-insensitive and unknown members are ignored.</summary>
    public sealed record CreateCustomerRequestDto
    {
        /// <summary>External account reference. Required and nonblank; unique within the Customer or Admin collection.</summary>
        /// <example>account-123</example>
        [property: JsonRequired] public required string ExternalId { get; init; }
        /// <summary>Complete individual profile. The member is required; explicit null returns a required-object validation error.</summary>
        /// <example>{&quot;firstName&quot;:&quot;Anna&quot;,&quot;lastName&quot;:&quot;Kowalska&quot;,&quot;email&quot;:&quot;anna@example.com&quot;,&quot;phone&quot;:&quot;123456789&quot;,&quot;billingAddress&quot;:{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null},&quot;shippingAddress&quot;:{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}}</example>
        [property: JsonRequired] public required IndividualDataRequestDto Individual { get; init; }
        /// <summary>Company profiles. On creation, omission or JSON null is normalized to an empty collection.</summary>
        /// <example>[{&quot;taxId&quot;:&quot;1234567890&quot;,&quot;companyName&quot;:&quot;Example Company&quot;,&quot;billingAddress&quot;:{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null},&quot;shippingAddress&quot;:{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}}]</example>
        public IReadOnlyCollection<AddCompanyRequestDto>? Companies { get; init; } = [];
    }
}


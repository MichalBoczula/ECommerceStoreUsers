using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    /// <summary>Complete company replacement input for the identified company. Unknown members are ignored; names match case-insensitively.</summary>
    public sealed record UpdateCompanyRequestDto
    {
        /// <summary>Company tax identifier checked by the active business rule and unique within one Customer aggregate.</summary>
        /// <example>1234567890</example>
        [property: JsonRequired] public required string TaxId { get; init; }
        /// <summary>Company name checked by the active business rule.</summary>
        /// <example>Example Company</example>
        [property: JsonRequired] public required string CompanyName { get; init; }
        /// <summary>Complete billing address. The member is required; explicit null returns a required-object validation error.</summary>
        /// <example>{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}</example>
        [property: JsonRequired] public required AddressRequestDto BillingAddress { get; init; }
        /// <summary>Complete shipping address. The member is required; explicit null returns a required-object validation error.</summary>
        /// <example>{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}</example>
        [property: JsonRequired] public required AddressRequestDto ShippingAddress { get; init; }
    }
}


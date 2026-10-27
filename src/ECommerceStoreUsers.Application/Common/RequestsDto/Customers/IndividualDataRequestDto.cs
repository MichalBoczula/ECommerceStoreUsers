using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    /// <summary>Individual profile input. Required objects are checked by mapping and scalar values by business policies. Unknown members are ignored; names match case-insensitively.</summary>
    public sealed record IndividualDataRequestDto
    {
        /// <summary>Individual first name; the active policy requires a nonblank value containing a letter.</summary>
        /// <example>Anna</example>
        [property: JsonRequired] public required string FirstName { get; init; }
        /// <summary>Individual last name; the active policy requires a nonblank value containing a letter.</summary>
        /// <example>Kowalska</example>
        [property: JsonRequired] public required string LastName { get; init; }
        /// <summary>Email text checked by the active business email rule. This is not a claim of full RFC email validation.</summary>
        /// <example>anna@example.com</example>
        [property: JsonRequired] public required string Email { get; init; }
        /// <summary>Phone text containing 7 to 10 digits under the active business rule.</summary>
        /// <example>123456789</example>
        [property: JsonRequired] public required string Phone { get; init; }
        /// <summary>Complete billing address. The member is required; explicit null returns a required-object validation error.</summary>
        /// <example>{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}</example>
        [property: JsonRequired] public required AddressRequestDto BillingAddress { get; init; }
        /// <summary>Complete shipping address. The member is required; explicit null returns a required-object validation error.</summary>
        /// <example>{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}</example>
        [property: JsonRequired] public required AddressRequestDto ShippingAddress { get; init; }
    }
}


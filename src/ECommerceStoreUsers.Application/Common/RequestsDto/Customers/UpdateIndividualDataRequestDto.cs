using System.Text.Json.Serialization;

namespace ECommerceStoreUsers.Application.Common.RequestsDto.Customers
{
    /// <summary>Complete individual replacement input. Unknown members are ignored; names match case-insensitively.</summary>
    public sealed record UpdateIndividualDataRequestDto
    {
        /// <summary>Complete individual profile. The member is required; explicit null returns a required-object validation error.</summary>
        /// <example>{&quot;firstName&quot;:&quot;Anna&quot;,&quot;lastName&quot;:&quot;Kowalska&quot;,&quot;email&quot;:&quot;anna@example.com&quot;,&quot;phone&quot;:&quot;123456789&quot;,&quot;billingAddress&quot;:{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null},&quot;shippingAddress&quot;:{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}}</example>
        [property: JsonRequired] public required IndividualDataRequestDto Individual { get; init; }
    }
}


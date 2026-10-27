namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Customers
{
    /// <summary>Complete current individual profile and both addresses.</summary>
    public sealed record IndividualDataResponseDto
    {
        /// <summary>Individual first name; the active policy requires a nonblank value containing a letter.</summary>
        /// <example>Anna</example>
        public required string FirstName { get; init; }
        /// <summary>Individual last name; the active policy requires a nonblank value containing a letter.</summary>
        /// <example>Kowalska</example>
        public required string LastName { get; init; }
        /// <summary>Email text checked by the active business email rule. This is not a claim of full RFC email validation.</summary>
        /// <example>anna@example.com</example>
        public required string Email { get; init; }
        /// <summary>Phone text containing 7 to 10 digits under the active business rule.</summary>
        /// <example>123456789</example>
        public required string Phone { get; init; }
        /// <summary>Complete current billing address.</summary>
        /// <example>{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}</example>
        public required AddressResponseDto BillingAddress { get; init; }
        /// <summary>Complete current shipping address.</summary>
        /// <example>{&quot;postalCode&quot;:&quot;00-001&quot;,&quot;city&quot;:&quot;Warsaw&quot;,&quot;street&quot;:&quot;Example Street&quot;,&quot;buildingNumber&quot;:&quot;10&quot;,&quot;apartmentNumber&quot;:null}</example>
        public required AddressResponseDto ShippingAddress { get; init; }
    }
}


namespace ECommerceStoreUsers.Application.Common.ResponsesDto.Customers
{
    /// <summary>Complete current address. apartmentNumber is always emitted and may be null.</summary>
    public sealed record AddressResponseDto
    {
        /// <summary>Postal code text checked by the active address rule.</summary>
        /// <example>00-001</example>
        public required string PostalCode { get; init; }
        /// <summary>City text checked by the active address rule.</summary>
        /// <example>Warsaw</example>
        public required string City { get; init; }
        /// <summary>Street text; null, empty and whitespace-only values fail the active rule.</summary>
        /// <example>Example Street</example>
        public required string Street { get; init; }
        /// <summary>Building number text checked by the active address rule.</summary>
        /// <example>10</example>
        public required string BuildingNumber { get; init; }
        /// <summary>Member must be present. JSON null represents no apartment; a string is checked by the active apartment rule.</summary>
        /// <example>null</example>
        public required string? ApartmentNumber { get; init; }
    }
}


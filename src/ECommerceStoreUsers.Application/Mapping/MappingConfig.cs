using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;

namespace ECommerceStoreUsers.Application.Mapping;

internal static class MappingConfig
{
    internal static Customer MapToDomain(CreateCustomerRequestDto request)
    {
        var individualData = MapToDomain(request.Individual);
        return new Customer(request.ExternalId, individualData);
    }

    internal static IndividualData MapToDomain(IndividualDataRequestDto dto)
    {
        return new IndividualData(
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.Phone,
            MapAddress(dto.BillingAddress),
            MapAddress(dto.ShippingAddress)
        );
    }

    internal static Address MapAddress(AddressRequestDto dto)
    {
        return new Address(
            dto.PostalCode,
            dto.City,
            dto.Street,
            dto.BuildingNumber,
            dto.ApartmentNumber
        );
    }

    internal static CustomerResponseDto MapToResponse(Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            ExternalId = customer.ExternalId,
            UpdatedAt = customer.UpdatedAt,
            Individual = MapToResponse(customer.Individual),
            Companies = customer.Companies.Select(MapToResponse).ToList()
        };
    }

    internal static IndividualDataResponseDto MapToResponse(IndividualData individual)
    {
        return new IndividualDataResponseDto
        {
            FirstName = individual.FirstName,
            LastName = individual.LastName,
            Email = individual.Email,
            Phone = individual.Phone,
            BillingAddress = MapToResponse(individual.BillingAddress),
            ShippingAddress = MapToResponse(individual.ShippingAddress)
        };
    }

    internal static CompanyDataResponseDto MapToResponse(CompanyData company)
    {
        return new CompanyDataResponseDto
        {
            Id = company.Id,
            TaxId = company.TaxId,
            CompanyName = company.CompanyName,
            BillingAddress = MapToResponse(company.BillingAddress),
            ShippingAddress = MapToResponse(company.ShippingAddress)
        };
    }

    internal static AddressResponseDto MapToResponse(Address address)
    {
        return new AddressResponseDto
        {
            PostalCode = address.PostalCode,
            City = address.City,
            Street = address.Street,
            BuildingNumber = address.BuildingNumber,
            ApartmentNumber = address.ApartmentNumber
        };
    }
}
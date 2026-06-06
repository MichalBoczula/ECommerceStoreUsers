using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.CreateCustomerConflictError
{
    public sealed class CreateCustomerDuplicateCompaniesEndpointTests : IClassFixture<ApplicationFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public CreateCustomerDuplicateCompaniesEndpointTests(ApplicationFactory factory)
        {
            _httpClient = factory.CreateClient();
        }

        [Fact]
        public async Task CreateCustomer_WhenCompanyListContainsDuplicatedTaxId_ShouldReturnConflict()
        {
            // Arrange
            var request = CreateCustomerRequest();

            // Act
            var response = await _httpClient.PostAsJsonAsync("/customers", request, _jsonOptions);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

            var problemDetails = await response.Content.ReadFromJsonAsync<ConflictProblemDetails>(_jsonOptions);
            problemDetails.ShouldNotBeNull();
            problemDetails!.Status.ShouldBe((int)HttpStatusCode.Conflict);
            problemDetails.Title.ShouldBe("Conflict.");
            problemDetails.Instance.ShouldBe("/customers");
            problemDetails.Detail.ShouldContain("Resource CompanyData identified by id 1234567890 already exists in db. Error in action CreateCustomer.");
            problemDetails.TraceId.ShouldNotBeNullOrWhiteSpace();
        }

        private static CreateCustomerRequestDto CreateCustomerRequest()
        {
            return new CreateCustomerRequestDto
            {
                ExternalId = $"auth-customer-duplicate-company-{Guid.NewGuid():N}",
                Individual = new IndividualDataRequestDto
                {
                    FirstName = "Jane",
                    LastName = "CompanyDuplicate",
                    Email = "jane.companyduplicate@example.com",
                    Phone = "987654321",
                    BillingAddress = CreateAddressRequest("01-001", "Warsaw", "Conflict Street", "11", "21"),
                    ShippingAddress = CreateAddressRequest("01-002", "Krakow", "Duplicate Avenue", "16", "26")
                },
                Companies = new List<AddCompanyRequestDto>
                {
                    CreateCompanyRequest("First Company", "1234567890"),
                    CreateCompanyRequest("Second Company", "1234567890")
                }
            };
        }

        private static AddCompanyRequestDto CreateCompanyRequest(string companyName, string taxId)
        {
            return new AddCompanyRequestDto
            {
                CompanyName = companyName,
                TaxId = taxId,
                BillingAddress = CreateAddressRequest("30-300", "Poznan", "Business", "3", "15"),
                ShippingAddress = CreateAddressRequest("40-400", "Lodz", "Industry", "4", "16")
            };
        }

        private static AddressRequestDto CreateAddressRequest(
            string postalCode,
            string city,
            string street,
            string buildingNumber,
            string apartmentNumber)
        {
            return new AddressRequestDto
            {
                PostalCode = postalCode,
                City = city,
                Street = street,
                BuildingNumber = buildingNumber,
                ApartmentNumber = apartmentNumber
            };
        }
    }
}

using ECommerceStoreUsers.API.Configuration.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
using ECommerceStoreUsers.Application.Services.Abstract.Customers;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceStoreUsers.API.Endpoints
{
    public static class CustomersEndpoints
    {
        public static IEndpointRouteBuilder MapCustomersEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/customers").WithTags("Customers");

            MapCustomersQueries(group);
            MapCustomersCommands(group);

            return group;
        }

        private static void MapCustomersCommands(IEndpointRouteBuilder group)
        {
            group.MapPost("/", async (
                CreateCustomerRequestDto request,
                ICustomerService customerService,
                CancellationToken cancellationToken) =>
            {
                var customer = await customerService.CreateCustomer(request, cancellationToken);

                return Results.Ok(customer);
            })
            .WithSummary("Create customer profile.")
            .WithDescription("Creates a new customer aggregate record complete with individual user data details and optional company metadata. " +
                "Explicit null required objects return 400 validation_failed with a RequiredRequestObject error: " +
                "entity IndividualData, message 'individual is required.'; or entity Address, message 'billingAddress is required.' or 'shippingAddress is required.'. " +
                "A confirmed ExternalId uniqueness race returns 409 resource_conflict; a transaction write conflict returns 409 concurrency_conflict.")
            .WithName("CreateCustomer")
            .Produces<CustomerResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");

            group.MapPut("/{id:guid}/individual", async (
                Guid id,
                UpdateIndividualDataRequestDto request,
                ICustomerService customerService,
                CancellationToken cancellationToken) =>
            {
                var customer = await customerService.UpdateIndividualData(id, request, cancellationToken);

                return Results.Ok(customer);
            })
            .WithSummary("Update individual personal data.")
            .WithDescription("Updates individual details. A concurrent customer change returns 409; a customer removed before saving returns 404. " +
                "Explicit null required objects return 400 validation_failed with a RequiredRequestObject error: " +
                "entity IndividualData, message 'individual is required.'; or entity Address, message 'billingAddress is required.' or 'shippingAddress is required.'.")
            .WithName("UpdateIndividualData")
            .Produces<CustomerResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");

            group.MapPost("/{customerId:guid}/companies", async (
                Guid customerId,
                AddCompanyRequestDto request,
                ICustomerService customerService,
                CancellationToken cancellationToken) =>
            {
                var customer = await customerService.AddCompany(customerId, request, cancellationToken);

                return Results.Ok(customer);
            })
            .WithSummary("Add company metadata.")
            .WithDescription("Adds company data. Duplicate tax IDs and concurrent customer changes return 409; a customer removed before saving returns 404. " +
                "Explicit null addresses return 400 validation_failed with a RequiredRequestObject error, entity Address, " +
                "and message 'billingAddress is required.' or 'shippingAddress is required.'.")
            .WithName("AddCompany")
            .Produces<CustomerResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");

            group.MapPut("/{customerId:guid}/companies/{companyId:guid}", async (
                Guid customerId,
                Guid companyId,
                UpdateCompanyRequestDto request,
                ICustomerService customerService,
                CancellationToken cancellationToken) =>
            {
                var customer = await customerService.UpdateCompany(customerId, companyId, request, cancellationToken);

                return Results.Ok(customer);
            })
            .WithSummary("Update one Customer company.")
            .WithDescription("Updates company data. Duplicate tax IDs and concurrent customer changes return 409; a customer removed before saving returns 404. " +
                "Explicit null addresses return 400 validation_failed with a RequiredRequestObject error, entity Address, " +
                "and message 'billingAddress is required.' or 'shippingAddress is required.'.")
            .WithName("UpdateCompany")
            .Produces<CustomerResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");
        }

        private static void MapCustomersQueries(IEndpointRouteBuilder group)
        {
            group.MapGet("/external/{externalId}", async (
                string externalId,
                ICustomerService customerService,
                CancellationToken cancellationToken) =>
            {
                var customer = await customerService.GetCustomerByExternalId(externalId, cancellationToken);

                return Results.Ok(customer);
            })
            .WithSummary("Get Customer by external account reference.")
            .WithDescription("Returns the complete current individual and company profile for the external account reference. Missing profiles return 404; this lookup does not change stored data.")
            .WithName("GetCustomerByExternalId")
            .Produces<CustomerResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");
        }
    }
}


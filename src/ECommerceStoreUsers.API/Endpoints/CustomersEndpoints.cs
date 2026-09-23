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
            .WithDescription("Creates a new customer aggregate record complete with individual user data details and optional company metadata.")
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
            .WithDescription("Updates individual details. A concurrent customer change returns 409; a customer removed before saving returns 404.")
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
            .WithDescription("Adds company data. Duplicate tax IDs and concurrent customer changes return 409; a customer removed before saving returns 404.")
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
            .WithSummary("Update specific corporate company elements.")
            .WithDescription("Updates company data. Duplicate tax IDs and concurrent customer changes return 409; a customer removed before saving returns 404.")
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
            .WithSummary("Get customer profile data by identity engine pointer.")
            .WithDescription("Returns a flattened clean data view context assigned underneath a specific global external account provider identification hash sequence format block.")
            .WithName("GetCustomerByExternalId")
            .Produces<CustomerResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");
        }
    }
}

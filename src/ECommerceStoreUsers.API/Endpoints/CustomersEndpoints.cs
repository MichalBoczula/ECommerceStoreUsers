using ECommerceStoreInvoice.API.Configuration.Common;
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
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ConflictProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

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
            .WithDescription("Modifies existing core individual metrics (names, contact info, billing/shipping directions) for the target customer profile identifier.")
            .WithName("UpdateIndividualData")
            .Produces<CustomerResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<NotFoundProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

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
            .WithDescription("Appends a newly structured commercial corporate tax record entity into the internal collection profile context.")
            .WithName("AddCompany")
            .Produces<CustomerResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<NotFoundProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ConflictProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

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
            .WithDescription("Modifies naming, tax id registration credentials, and billing/shipping information records assigned onto a specific tracking sub-company context component block.")
            .WithName("UpdateCompany")
            .Produces<CustomerResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<NotFoundProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
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
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<NotFoundProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using ECommerceStoreUsers.API.Configuration.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Application.Services.Abstract.Admins;

namespace ECommerceStoreUsers.API.Endpoints
{
    public static class AdminsEndpoints
    {
        public static IEndpointRouteBuilder MapAdminsEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/admins").WithTags("Admins");

            MapAdminsQueries(group);
            MapAdminsCommands(group);

            return group;
        }

        private static void MapAdminsCommands(IEndpointRouteBuilder group)
        {
            group.MapPost("/", async (
                CreateAdminRequestDto request,
                IAdminProfileService adminService,
                CancellationToken cancellationToken) =>
            {
                var admin = await adminService.CreateAdmin(request, cancellationToken);

                return Results.Ok(admin);
            })
            .WithSummary("Create admin profile.")
            .WithDescription("Creates a new administrator workspace account profile.")
            .WithName("CreateAdmin")
            .Produces<AdminResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");

            group.MapPut("/{adminId:guid}", async (
                Guid adminId,
                UpdateAdminProfileRequestDto request,
                IAdminProfileService adminService,
                CancellationToken cancellationToken) =>
            {
                var admin = await adminService.UpdateAdminProfile(adminId, request, cancellationToken);

                return Results.Ok(admin);
            })
            .WithSummary("Update admin profile metrics.")
            .WithDescription("Updates administrator name and email. An unchanged profile is a successful no-op. A concurrent change returns 409; an administrator removed before saving returns 404.")
            .WithName("UpdateAdminProfile")
            .Produces<AdminResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");
        }

        private static void MapAdminsQueries(IEndpointRouteBuilder group)
        {
            group.MapGet("/external/{externalId}", async (
                string externalId,
                IAdminProfileService adminService,
                CancellationToken cancellationToken) =>
            {
                var admin = await adminService.GetAdminByExternalId(externalId, cancellationToken);

                return Results.Ok(admin);
            })
            .WithSummary("Get admin profile by external identification pointer.")
            .WithDescription("Returns a flat administrative account overview context assigned beneath a specific identity engine reference string token context.")
            .WithName("GetAdminByExternalId")
            .Produces<AdminResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ApiProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");
        }
    }
}

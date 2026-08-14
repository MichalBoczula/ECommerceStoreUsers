using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Common.ResponsesDto;
using ECommerceStoreUsers.Application.Services.Abstract.Admins;
using ECommerceStoreUsers.Application.Services.Abstract.Customers;
using ECommerceStoreUsers.Application.Services.Abstract.Favorites;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceStoreUsers.API.Endpoints
{
    public static class DocumentationEndpoints
    {
        public static IEndpointRouteBuilder MapDocumentationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/users-documentation").WithTags("Documentation");

            MapFlowDocumentation(group);
            MapValidationDocumentation(group);

            return group;
        }

        private static void MapFlowDocumentation(IEndpointRouteBuilder group)
        {
            group.MapGet("/flows", (
                ICustomerDescriptorService customerDescriptorService,
                IAdminFlowDescriptorService adminFlowDescriptorService,
                IFavoriteFlowDescriptorService favoriteFlowDescriptorService) =>
            {
                var response = new FlowDescriptorsResponseDto
                {
                    Flows =
                    [
                       new Dictionary<string, FlowDescriptor>
                        {
                            // Customer Flows
                            [nameof(customerDescriptorService.GetCreateCustomerDescriptor)] = customerDescriptorService.GetCreateCustomerDescriptor(),
                            [nameof(customerDescriptorService.GetCustomerByExternalIdDescriptor)] = customerDescriptorService.GetCustomerByExternalIdDescriptor(),
                            [nameof(customerDescriptorService.GetUpdateIndividualDataDescriptor)] = customerDescriptorService.GetUpdateIndividualDataDescriptor(),
                            [nameof(customerDescriptorService.GetAddCompanyDescriptor)] = customerDescriptorService.GetAddCompanyDescriptor(),
                            [nameof(customerDescriptorService.GetUpdateCompanyDescriptor)] = customerDescriptorService.GetUpdateCompanyDescriptor(),

                            // Admin Flows
                            [nameof(adminFlowDescriptorService.GetGetAdminByExternalIdDescriptor)] = adminFlowDescriptorService.GetGetAdminByExternalIdDescriptor(),
                            [nameof(adminFlowDescriptorService.GetCreateAdminDescriptor)] = adminFlowDescriptorService.GetCreateAdminDescriptor(),
                            [nameof(adminFlowDescriptorService.GetUpdateAdminProfileDescriptor)] = adminFlowDescriptorService.GetUpdateAdminProfileDescriptor(),

                            // Favorite Flows
                            [nameof(favoriteFlowDescriptorService.GetGetFavoritesByClientIdDescriptor)] = favoriteFlowDescriptorService.GetGetFavoritesByClientIdDescriptor(),
                            [nameof(favoriteFlowDescriptorService.GetAddProductToFavoritesDescriptor)] = favoriteFlowDescriptorService.GetAddProductToFavoritesDescriptor(),
                            [nameof(favoriteFlowDescriptorService.GetRemoveProductFromFavoritesDescriptor)] = favoriteFlowDescriptorService.GetRemoveProductFromFavoritesDescriptor(),
                            [nameof(favoriteFlowDescriptorService.GetClearClientFavoritesDescriptor)] = favoriteFlowDescriptorService.GetClearClientFavoritesDescriptor()
                        }
                    ]
                };

                return Results.Ok(response);
            })
            .WithSummary("Get flow documentation.")
            .WithDescription("Returns flow descriptors mapped by descriptor name, including customer, admin, and favorite workflows.")
            .WithName("GetFlowDocumentation")
            .Produces<FlowDescriptorsResponseDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        }

        private static void MapValidationDocumentation(IEndpointRouteBuilder group)
        {
            group.MapGet("/validations", (IEnumerable<IValidationPolicyDescriptorProvider> validationDescriptorProviders) =>
            {
                var validationDescriptors = validationDescriptorProviders
                    .Select(provider => provider.Describe())
                    .Select(descriptor => new Dictionary<string, ValidationPolicyDescriptor>
                    {
                        [descriptor.PolicyName] = descriptor
                    })
                    .ToList();

                var response = new ValidationDescriptorsResponseDto
                {
                    Validations = validationDescriptors
                };

                return Results.Ok(response);
            })
            .WithSummary("Get validation documentation.")
            .WithDescription("Returns validation descriptors mapped by policy name.")
            .WithName("GetValidationDocumentation")
            .Produces<ValidationDescriptorsResponseDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        }
    }
}

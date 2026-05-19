using ECommerceStoreUsers.Application.Common.ResponsesDto;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceStoreUsers.API.Endpoints
{
    public static class DocumentationEndpoints
    {
        public static IEndpointRouteBuilder MapDocumentationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/documentation").WithTags("Documentation");

            MapFlowDocumentation(group);
            MapValidationDocumentation(group);

            return group;
        }

        private static void MapFlowDocumentation(IEndpointRouteBuilder group)
        {
            group.MapGet("/flows", () =>
            {
                return Results.Ok();
            })
            .WithSummary("Get flow documentation.")
            .WithDescription("Returns flow descriptors mapped by descriptor name.")
            .WithName("GetFlowDocumentation")
            .Produces(StatusCodes.Status200OK)
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

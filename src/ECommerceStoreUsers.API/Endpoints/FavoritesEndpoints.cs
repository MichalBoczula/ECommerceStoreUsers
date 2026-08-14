using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Favorites;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Favorites;
using ECommerceStoreUsers.Application.Services.Abstract.Favorites;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceStoreUsers.API.Endpoints
{
    public static class FavoritesEndpoints
    {
        public static IEndpointRouteBuilder MapFavoritesEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/favorites").WithTags("Favorites");

            MapFavoritesQueries(group);
            MapFavoritesCommands(group);

            return group;
        }

        private static void MapFavoritesQueries(IEndpointRouteBuilder group)
        {
            group.MapGet("/clients/{clientId:guid}", async (
                Guid clientId,
                IFavoriteService favoriteService,
                CancellationToken cancellationToken) =>
            {
                var favorites = await favoriteService.GetFavoritesByClientId(clientId, cancellationToken);
                return Results.Ok(favorites);
            })
            .WithSummary("Get customer favorites list.")
            .WithDescription("Retrieves all favorited product entries for a specified client identifier.")
            .WithName("GetFavoritesByClientId")
            .Produces<IReadOnlyList<FavoriteResponseDto>>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        }

        private static void MapFavoritesCommands(IEndpointRouteBuilder group)
        {
            group.MapPost("/clients/{clientId:guid}", async (
                Guid clientId,
                AddFavoriteRequestDto request,
                IFavoriteService favoriteService,
                CancellationToken cancellationToken) =>
            {
                var favorite = await favoriteService.AddProductToFavorites(clientId, request, cancellationToken);
                return Results.Ok(favorite);
            })
            .WithSummary("Add product to favorites.")
            .WithDescription("Adds a product reference to the customer's favorites list.")
            .WithName("AddProductToFavorites")
            .Produces<FavoriteResponseDto>(StatusCodes.Status200OK)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ConflictProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

            group.MapDelete("/clients/{clientId:guid}/products/{productId:guid}", async (
                Guid clientId,
                Guid productId,
                IFavoriteService favoriteService,
                CancellationToken cancellationToken) =>
            {
                await favoriteService.RemoveProductFromFavorites(clientId, productId, cancellationToken);
                return Results.NoContent();
            })
            .WithSummary("Remove product from favorites.")
            .WithDescription("Removes a specific product from the client's favorites.")
            .WithName("RemoveProductFromFavorites")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<NotFoundProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

            group.MapDelete("/clients/{clientId:guid}", async (
                Guid clientId,
                IFavoriteService favoriteService,
                CancellationToken cancellationToken) =>
            {
                await favoriteService.ClearClientFavorites(clientId, cancellationToken);
                return Results.NoContent();
            })
            .WithSummary("Clear all customer favorites.")
            .WithDescription("Removes all saved product favorites for a specific client.")
            .WithName("ClearClientFavorites")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
        }
    }
}

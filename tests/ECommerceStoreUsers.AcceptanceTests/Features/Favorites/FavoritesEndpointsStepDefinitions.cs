using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Favorites;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Favorites;
using Reqnroll;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Favorites
{
    [Binding]
    public sealed class FavoritesEndpointsStepDefinitions
    {
        private readonly ScenarioApiContext _apiContext;
        private Guid _clientId;
        private Guid _productId;

        public FavoritesEndpointsStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("I use favorite identifiers")]
        public void GivenIUseFavoriteIdentifiers(Table table)
        {
            var values = table.Rows.ToDictionary(row => row["Field"], row => row["Value"]);
            _clientId = Guid.Parse(values["ClientId"]);
            _productId = Guid.Parse(values["ProductId"]);
        }

        [Given("the product is already in favorites")]
        public async Task GivenTheProductIsAlreadyInFavorites()
        {
            await AddFavoriteAndRequireSuccess(_productId);
        }

        [Given("the client has two favorite products")]
        public async Task GivenTheClientHasTwoFavoriteProducts(Table table)
        {
            foreach (var row in table.Rows)
            {
                await AddFavoriteAndRequireSuccess(Guid.Parse(row["ProductId"]));
            }
        }

        [When("I add the product to favorites")]
        public async Task WhenIAddTheProductToFavorites()
        {
            _apiContext.Response = await AddFavorite(_productId);
        }

        [When("I add a favorite without the required product identifier")]
        public async Task WhenIAddAFavoriteWithoutTheRequiredProductIdentifier()
        {
            using var content = new StringContent("{}", Encoding.UTF8, "application/json");
            _apiContext.Response = await _apiContext.HttpClient.PostAsync(
                $"/favorites/clients/{_clientId}",
                content);
        }

        [When("I add the same product to favorites twice")]
        public async Task WhenIAddTheSameProductToFavoritesTwice()
        {
            await AddFavoriteAndRequireSuccess(_productId);
            _apiContext.Response = await AddFavorite(_productId);
        }

        [When("I get the client's favorites")]
        public async Task WhenIGetTheClientsFavorites()
        {
            _apiContext.Response = await _apiContext.HttpClient.GetAsync(
                $"/favorites/clients/{_clientId}");
        }

        [When("I remove the product from favorites")]
        public async Task WhenIRemoveTheProductFromFavorites()
        {
            _apiContext.Response = await _apiContext.HttpClient.DeleteAsync(
                $"/favorites/clients/{_clientId}/products/{_productId}");
        }

        [When("I clear the client's favorites")]
        public async Task WhenIClearTheClientsFavorites()
        {
            _apiContext.Response = await _apiContext.HttpClient.DeleteAsync(
                $"/favorites/clients/{_clientId}");
        }

        [Then("the added favorite is returned with status 200")]
        public async Task ThenTheAddedFavoriteIsReturnedWithStatus200()
        {
            RequireResponse(HttpStatusCode.OK, "application/json");

            var favorite = await Deserialize<FavoriteResponseDto>();
            favorite.ShouldNotBeNull();
            favorite.ClientId.ShouldBe(_clientId);
            favorite.ProductId.ShouldBe(_productId);
            favorite.Id.ShouldNotBe(Guid.Empty);
            favorite.AddedAt.ShouldNotBe(default);
        }

        [Then("the favorite list is returned with status 200")]
        public async Task ThenTheFavoriteListIsReturnedWithStatus200()
        {
            RequireResponse(HttpStatusCode.OK, "application/json");

            var favorites = await Deserialize<List<FavoriteResponseDto>>();
            favorites.ShouldNotBeNull();
            favorites.Count.ShouldBe(1);
            favorites[0].ClientId.ShouldBe(_clientId);
            favorites[0].ProductId.ShouldBe(_productId);
        }

        [Then("favorite validation fails with status 400")]
        public async Task ThenFavoriteValidationFailsWithStatus400(Table table)
        {
            RequireResponse(HttpStatusCode.BadRequest, "application/problem+json");

            var problem = await Deserialize<ApiProblemDetails>();
            problem.ShouldNotBeNull();
            problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
            problem.Title.ShouldBe("Validation failed.");
            problem.Instance.ShouldBe(_apiContext.Response!.RequestMessage!.RequestUri!.AbsolutePath);
            problem.TraceId.ShouldNotBeNullOrWhiteSpace();

            var actualErrors = problem.Errors.ToList();
            actualErrors.Count.ShouldBe(table.Rows.Count);

            foreach (var expected in table.Rows)
            {
                actualErrors.ShouldContain(error =>
                    error.Name == expected["Name"] &&
                    error.Message == expected["Message"]);
            }
        }

        [Then("the malformed favorite payload fails with status 400")]
        public async Task ThenTheMalformedFavoritePayloadFailsWithStatus400()
        {
            RequireResponse(HttpStatusCode.BadRequest, "application/problem+json");

            var problem = await Deserialize<ApiProblemDetails>();
            problem.ShouldNotBeNull();
            problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
            problem.Title.ShouldBe("Invalid JSON payload.");
            problem.Instance.ShouldBe($"/favorites/clients/{_clientId}");
            problem.TraceId.ShouldNotBeNullOrWhiteSpace();
            problem.MissingProperties.ShouldContain("ProductId");
        }

        [Then("adding the duplicate favorite fails with status 409")]
        public async Task ThenAddingTheDuplicateFavoriteFailsWithStatus409()
        {
            RequireResponse(HttpStatusCode.Conflict, "application/problem+json");

            var problem = await Deserialize<ConflictProblemDetails>();
            problem.ShouldNotBeNull();
            problem.Status.ShouldBe(StatusCodes.Status409Conflict);
            problem.Title.ShouldBe("Conflict.");
            problem.Instance.ShouldBe($"/favorites/clients/{_clientId}");
            problem.TraceId.ShouldNotBeNullOrWhiteSpace();
            problem.Detail.ShouldContain($"{_clientId}:{_productId}");
        }

        [Then("the favorite is removed with status 204")]
        public async Task ThenTheFavoriteIsRemovedWithStatus204()
        {
            RequireResponse(HttpStatusCode.NoContent);

            using var verificationResponse = await _apiContext.HttpClient.GetAsync(
                $"/favorites/clients/{_clientId}");
            verificationResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var favorites = await verificationResponse.Content.ReadFromJsonAsync<List<FavoriteResponseDto>>(
                _apiContext.JsonOptions);
            favorites.ShouldNotBeNull();
            favorites.ShouldBeEmpty();
        }

        [Then("the missing favorite fails with status 404")]
        public async Task ThenTheMissingFavoriteFailsWithStatus404()
        {
            RequireResponse(HttpStatusCode.NotFound, "application/problem+json");

            var problem = await Deserialize<NotFoundProblemDetails>();
            problem.ShouldNotBeNull();
            problem.Status.ShouldBe(StatusCodes.Status404NotFound);
            problem.Title.ShouldBe("Resource not found.");
            problem.Instance.ShouldBe($"/favorites/clients/{_clientId}/products/{_productId}");
            problem.TraceId.ShouldNotBeNullOrWhiteSpace();
            problem.Detail.ShouldContain($"{_clientId}:{_productId}");
        }

        [Then("all favorites are removed with status 204")]
        public async Task ThenAllFavoritesAreRemovedWithStatus204()
        {
            RequireResponse(HttpStatusCode.NoContent);

            using var verificationResponse = await _apiContext.HttpClient.GetAsync(
                $"/favorites/clients/{_clientId}");
            verificationResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var favorites = await verificationResponse.Content.ReadFromJsonAsync<List<FavoriteResponseDto>>(
                _apiContext.JsonOptions);
            favorites.ShouldNotBeNull();
            favorites.ShouldBeEmpty();
        }

        private Task<HttpResponseMessage> AddFavorite(Guid productId)
        {
            return _apiContext.HttpClient.PostAsJsonAsync(
                $"/favorites/clients/{_clientId}",
                new AddFavoriteRequestDto
                {
                    ProductId = productId
                },
                _apiContext.JsonOptions);
        }

        private async Task AddFavoriteAndRequireSuccess(Guid productId)
        {
            using var response = await AddFavorite(productId);
            var body = await response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson($"Seed favorite response ({(int)response.StatusCode})", body);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        private void RequireResponse(HttpStatusCode statusCode, string? mediaType = null)
        {
            _apiContext.Response.ShouldNotBeNull();
            _apiContext.Response.StatusCode.ShouldBe(statusCode);

            if (mediaType is not null)
            {
                _apiContext.Response.Content.Headers.ContentType?.MediaType.ShouldBe(mediaType);
            }
        }

        private async Task<T?> Deserialize<T>()
        {
            _apiContext.Response.ShouldNotBeNull();

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson(
                $"Favorite response ({(int)_apiContext.Response.StatusCode})",
                body);

            return JsonSerializer.Deserialize<T>(body, _apiContext.JsonOptions);
        }
    }
}

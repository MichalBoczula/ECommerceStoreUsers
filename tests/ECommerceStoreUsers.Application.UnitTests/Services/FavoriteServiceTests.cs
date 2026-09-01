using ECommerceStoreUsers.Application.Common.RequestsDto.Favorites;
using ECommerceStoreUsers.Application.Services.Concrete.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace ECommerceStoreUsers.Application.UnitTests.Services;

public sealed class FavoriteServiceTests
{
    [Fact]
    public async Task GetFavoritesByClientId_WhenClientIdIsValid_ShouldReturnFavoriteResponses()
    {
        var clientId = Guid.NewGuid();
        var favorites = new List<Favorite>
        {
            new(clientId, Guid.NewGuid()),
            new(clientId, Guid.NewGuid())
        };
        var cancellationToken = CancellationToken.None;
        var (repositoryMock, favoritePolicyMock, guidPolicyMock, sut) = CreateSut();

        guidPolicyMock.Setup(policy => policy.Validate(clientId)).ReturnsAsync(new ValidationResult());
        repositoryMock.Setup(repository => repository.GetByClientIdAsync(clientId, cancellationToken)).ReturnsAsync(favorites);

        var result = await sut.GetFavoritesByClientId(clientId, cancellationToken);

        result.Count.ShouldBe(favorites.Count);
        result[0].Id.ShouldBe(favorites[0].Id);
        result[0].ClientId.ShouldBe(clientId);
        result[0].ProductId.ShouldBe(favorites[0].ProductId);
        result[0].AddedAt.ShouldBe(favorites[0].AddedAt);
        result[1].Id.ShouldBe(favorites[1].Id);
        guidPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        repositoryMock.Verify(repository => repository.GetByClientIdAsync(clientId, cancellationToken), Times.Once);
        favoritePolicyMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetFavoritesByClientId_WhenClientIdIsInvalid_ShouldThrowValidationException()
    {
        var clientId = Guid.Empty;
        var (repositoryMock, _, guidPolicyMock, sut) = CreateSut();
        guidPolicyMock.Setup(policy => policy.Validate(clientId)).ReturnsAsync(CreateInvalidResult());

        await Should.ThrowAsync<ValidationException>(() => sut.GetFavoritesByClientId(clientId, CancellationToken.None));

        guidPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        repositoryMock.Verify(repository => repository.GetByClientIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddProductToFavorites_WhenRequestIsValid_ShouldAddAndReturnFavoriteResponse()
    {
        var clientId = Guid.NewGuid();
        var request = new AddFavoriteRequestDto { ProductId = Guid.NewGuid() };
        var cancellationToken = CancellationToken.None;
        var (repositoryMock, favoritePolicyMock, guidPolicyMock, sut) = CreateSut();

        favoritePolicyMock
            .Setup(policy => policy.Validate(It.Is<Favorite>(favorite => favorite.ClientId == clientId && favorite.ProductId == request.ProductId)))
            .ReturnsAsync(new ValidationResult());
        repositoryMock.Setup(repository => repository.ExistsAsync(clientId, request.ProductId, cancellationToken)).ReturnsAsync(false);
        repositoryMock
            .Setup(repository => repository.AddAsync(
                It.Is<Favorite>(favorite => favorite.ClientId == clientId && favorite.ProductId == request.ProductId),
                cancellationToken))
            .Returns(Task.CompletedTask);

        var result = await sut.AddProductToFavorites(clientId, request, cancellationToken);

        result.Id.ShouldNotBe(Guid.Empty);
        result.ClientId.ShouldBe(clientId);
        result.ProductId.ShouldBe(request.ProductId);
        favoritePolicyMock.Verify(policy => policy.Validate(It.IsAny<Favorite>()), Times.Once);
        repositoryMock.Verify(repository => repository.ExistsAsync(clientId, request.ProductId, cancellationToken), Times.Once);
        repositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Favorite>(), cancellationToken), Times.Once);
        guidPolicyMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddProductToFavorites_WhenFavoriteIsInvalid_ShouldThrowValidationException()
    {
        var clientId = Guid.Empty;
        var request = new AddFavoriteRequestDto { ProductId = Guid.NewGuid() };
        var (repositoryMock, favoritePolicyMock, _, sut) = CreateSut();
        favoritePolicyMock.Setup(policy => policy.Validate(It.IsAny<Favorite>())).ReturnsAsync(CreateInvalidResult());

        await Should.ThrowAsync<ValidationException>(() => sut.AddProductToFavorites(clientId, request, CancellationToken.None));

        favoritePolicyMock.Verify(policy => policy.Validate(It.Is<Favorite>(favorite => favorite.ClientId == clientId)), Times.Once);
        repositoryMock.Verify(repository => repository.ExistsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Favorite>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddProductToFavorites_WhenFavoriteAlreadyExists_ShouldThrowResourceAlreadyExistsException()
    {
        var clientId = Guid.NewGuid();
        var request = new AddFavoriteRequestDto { ProductId = Guid.NewGuid() };
        var cancellationToken = CancellationToken.None;
        var (repositoryMock, favoritePolicyMock, _, sut) = CreateSut();
        favoritePolicyMock.Setup(policy => policy.Validate(It.IsAny<Favorite>())).ReturnsAsync(new ValidationResult());
        repositoryMock.Setup(repository => repository.ExistsAsync(clientId, request.ProductId, cancellationToken)).ReturnsAsync(true);

        await Should.ThrowAsync<ResourceAlreadyExistsException>(() => sut.AddProductToFavorites(clientId, request, cancellationToken));

        favoritePolicyMock.Verify(policy => policy.Validate(It.IsAny<Favorite>()), Times.Once);
        repositoryMock.Verify(repository => repository.ExistsAsync(clientId, request.ProductId, cancellationToken), Times.Once);
        repositoryMock.Verify(repository => repository.AddAsync(It.IsAny<Favorite>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveProductFromFavorites_WhenFavoriteExists_ShouldDeleteFavorite()
    {
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var favorite = new Favorite(clientId, productId);
        var cancellationToken = CancellationToken.None;
        var (repositoryMock, favoritePolicyMock, guidPolicyMock, sut) = CreateSut();
        guidPolicyMock.Setup(policy => policy.Validate(clientId)).ReturnsAsync(new ValidationResult());
        guidPolicyMock.Setup(policy => policy.Validate(productId)).ReturnsAsync(new ValidationResult());
        repositoryMock.Setup(repository => repository.GetByClientAndProductIdAsync(clientId, productId, cancellationToken)).ReturnsAsync(favorite);
        repositoryMock.Setup(repository => repository.DeleteAsync(clientId, productId, cancellationToken)).Returns(Task.CompletedTask);

        await sut.RemoveProductFromFavorites(clientId, productId, cancellationToken);

        guidPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        guidPolicyMock.Verify(policy => policy.Validate(productId), Times.Once);
        repositoryMock.Verify(repository => repository.GetByClientAndProductIdAsync(clientId, productId, cancellationToken), Times.Once);
        repositoryMock.Verify(repository => repository.DeleteAsync(clientId, productId, cancellationToken), Times.Once);
        favoritePolicyMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RemoveProductFromFavorites_WhenIdentifierIsInvalid_ShouldThrowValidationException()
    {
        var clientId = Guid.Empty;
        var productId = Guid.NewGuid();
        var (repositoryMock, _, guidPolicyMock, sut) = CreateSut();
        guidPolicyMock.Setup(policy => policy.Validate(clientId)).ReturnsAsync(CreateInvalidResult());
        guidPolicyMock.Setup(policy => policy.Validate(productId)).ReturnsAsync(new ValidationResult());

        await Should.ThrowAsync<ValidationException>(() => sut.RemoveProductFromFavorites(clientId, productId, CancellationToken.None));

        guidPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        guidPolicyMock.Verify(policy => policy.Validate(productId), Times.Once);
        repositoryMock.Verify(repository => repository.GetByClientAndProductIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(repository => repository.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveProductFromFavorites_WhenFavoriteDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var (repositoryMock, _, guidPolicyMock, sut) = CreateSut();
        guidPolicyMock.Setup(policy => policy.Validate(clientId)).ReturnsAsync(new ValidationResult());
        guidPolicyMock.Setup(policy => policy.Validate(productId)).ReturnsAsync(new ValidationResult());
        repositoryMock.Setup(repository => repository.GetByClientAndProductIdAsync(clientId, productId, cancellationToken)).ReturnsAsync((Favorite?)null);

        await Should.ThrowAsync<ResourceNotFoundException>(() => sut.RemoveProductFromFavorites(clientId, productId, cancellationToken));

        repositoryMock.Verify(repository => repository.GetByClientAndProductIdAsync(clientId, productId, cancellationToken), Times.Once);
        repositoryMock.Verify(repository => repository.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ClearClientFavorites_WhenClientIdIsValid_ShouldDeleteAllClientFavorites()
    {
        var clientId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var (repositoryMock, favoritePolicyMock, guidPolicyMock, sut) = CreateSut();
        guidPolicyMock.Setup(policy => policy.Validate(clientId)).ReturnsAsync(new ValidationResult());
        repositoryMock.Setup(repository => repository.DeleteAllByClientIdAsync(clientId, cancellationToken)).Returns(Task.CompletedTask);

        await sut.ClearClientFavorites(clientId, cancellationToken);

        guidPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        repositoryMock.Verify(repository => repository.DeleteAllByClientIdAsync(clientId, cancellationToken), Times.Once);
        favoritePolicyMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ClearClientFavorites_WhenClientIdIsInvalid_ShouldThrowValidationException()
    {
        var clientId = Guid.Empty;
        var (repositoryMock, _, guidPolicyMock, sut) = CreateSut();
        guidPolicyMock.Setup(policy => policy.Validate(clientId)).ReturnsAsync(CreateInvalidResult());

        await Should.ThrowAsync<ValidationException>(() => sut.ClearClientFavorites(clientId, CancellationToken.None));

        guidPolicyMock.Verify(policy => policy.Validate(clientId), Times.Once);
        repositoryMock.Verify(repository => repository.DeleteAllByClientIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static (Mock<IFavoriteRepository> RepositoryMock,
        Mock<IValidationPolicy<Favorite>> FavoritePolicyMock,
        Mock<IValidationPolicy<Guid>> GuidPolicyMock,
        FavoriteService Sut) CreateSut()
    {
        var repositoryMock = new Mock<IFavoriteRepository>(MockBehavior.Strict);
        var favoritePolicyMock = new Mock<IValidationPolicy<Favorite>>(MockBehavior.Strict);
        var guidPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<FavoriteService>>(MockBehavior.Loose);
        var sut = new FavoriteService(repositoryMock.Object, favoritePolicyMock.Object, guidPolicyMock.Object, loggerMock.Object);

        return (repositoryMock, favoritePolicyMock, guidPolicyMock, sut);
    }

    private static ValidationResult CreateInvalidResult()
    {
        var result = new ValidationResult();
        result.AddValidationError(new ValidationError
        {
            Entity = nameof(Favorite),
            Name = "Identifier",
            Message = "Identifier is required"
        });
        return result;
    }
}

using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Favorites;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Favorites.Favorites
{
    public class FavoriteValidationPolicyTests
    {
        [Fact]
        public async Task Validate_FavoriteWithInvalidClientId_ShouldReturnError()
        {
            // Arrange
            var policy = new FavoriteValidationPolicy();
            var favorite = new Favorite(Guid.Empty, Guid.NewGuid());

            // Act
            var result = await policy.Validate(favorite);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.GetValidationErrors().Count.ShouldBe(1);
            result.GetValidationErrors().ShouldContain(e => e.Name == "FavoriteClientIdValidationRule");
        }

        [Fact]
        public async Task Validate_FavoriteWithInvalidProductId_ShouldReturnError()
        {
            // Arrange
            var policy = new FavoriteValidationPolicy();
            var favorite = new Favorite(Guid.NewGuid(), Guid.Empty);

            // Act
            var result = await policy.Validate(favorite);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.GetValidationErrors().Count.ShouldBe(1);
            result.GetValidationErrors().ShouldContain(e => e.Name == "FavoriteProductIdValidationRule");
        }

        [Fact]
        public async Task Validate_FavoriteIsValid_ShouldReturnNoErrors()
        {
            // Arrange
            var policy = new FavoriteValidationPolicy();
            var favorite = new Favorite(Guid.NewGuid(), Guid.NewGuid());

            // Act
            var result = await policy.Validate(favorite);

            // Assert
            result.IsValid.ShouldBeTrue();
            result.GetValidationErrors().Count.ShouldBe(0);
        }
    }
}

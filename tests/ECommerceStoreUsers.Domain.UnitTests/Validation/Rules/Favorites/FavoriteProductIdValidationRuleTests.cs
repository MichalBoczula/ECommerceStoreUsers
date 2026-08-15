using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Favorites;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Favorites
{
    public class FavoriteProductIdValidationRuleTests
    {
        [Fact]
        public async Task IsValid_ProductIdIsEmpty_ShouldReturnError()
        {
            // Arrange
            var rule = new FavoriteProductIdValidationRule();
            var validationResult = new ValidationResult();
            var favorite = new Favorite(Guid.NewGuid(), Guid.Empty);

            // Act
            await rule.IsValid(favorite, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("ProductId cannot be an empty GUID.");
        }

        [Fact]
        public async Task IsValid_ProductIdIsNotEmpty_ShouldReturnNoErrors()
        {
            // Arrange
            var rule = new FavoriteProductIdValidationRule();
            var validationResult = new ValidationResult();
            var favorite = new Favorite(Guid.NewGuid(), Guid.NewGuid());

            // Act
            await rule.IsValid(favorite, validationResult);

            // Assert
            validationResult.GetValidationErrors().ShouldBeEmpty();
        }

        [Fact]
        public void Describe_ShouldReturnRuleDescriptors()
        {
            // Arrange
            var rule = new FavoriteProductIdValidationRule();

            // Act
            var descriptors = rule.Describe();

            // Assert
            descriptors.Count.ShouldBe(1);
            descriptors.ShouldContain(d => d.Message == "ProductId cannot be an empty GUID.");
            descriptors.ShouldContain(d => d.Name == nameof(FavoriteProductIdValidationRule));
            descriptors.ShouldContain(d => d.Entity == nameof(Favorite));
        }
    }
}

using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Favorites;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Favorites
{
    public class FavoriteClientIdValidationRuleTests
    {
        [Fact]
        public async Task IsValid_ClientIdIsEmpty_ShouldReturnError()
        {
            // Arrange
            var rule = new FavoriteClientIdValidationRule();
            var validationResult = new ValidationResult();
            var favorite = new Favorite(Guid.Empty, Guid.NewGuid());

            // Act
            await rule.IsValid(favorite, validationResult);

            // Assert
            validationResult.GetValidationErrors().Count.ShouldBe(1);
            validationResult.GetValidationErrors().First().Message.ShouldBe("ClientId cannot be an empty GUID.");
        }

        [Fact]
        public async Task IsValid_ClientIdIsNotEmpty_ShouldReturnNoErrors()
        {
            // Arrange
            var rule = new FavoriteClientIdValidationRule();
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
            var rule = new FavoriteClientIdValidationRule();

            // Act
            var descriptors = rule.Describe();

            // Assert
            descriptors.Count.ShouldBe(1);
            descriptors.ShouldContain(d => d.Message == "ClientId cannot be an empty GUID.");
            descriptors.ShouldContain(d => d.Name == nameof(FavoriteClientIdValidationRule));
            descriptors.ShouldContain(d => d.Entity == nameof(Favorite));
        }
    }
}

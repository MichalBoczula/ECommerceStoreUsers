using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Domain.Validation.Concrete.Rules.Common;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Rules.Common;

public class EmptyGuidRuleTests
{
    [Fact]
    public async Task IsValid_GuidIsEmpty_ShouldReturnError()
    {
        // Arrange
        var rule = new EmptyGuidRule();
        var validationResult = new ValidationResult();

        // Act
        await rule.IsValid(Guid.Empty, validationResult);

        // Assert
        validationResult.GetValidationErrors().Count.ShouldBe(1);
        validationResult.GetValidationErrors().First().Message.ShouldBe("Guid cannot be empty.");
    }

    [Fact]
    public async Task IsValid_GuidIsNotEmpty_ShouldNotReturnError()
    {
        // Arrange
        var rule = new EmptyGuidRule();
        var validationResult = new ValidationResult();

        // Act
        await rule.IsValid(Guid.NewGuid(), validationResult);

        // Assert
        validationResult.GetValidationErrors().Count.ShouldBe(0);
    }

    [Fact]
    public void Describe_ShouldReturnRuleDescriptors()
    {
        // Arrange
        var rule = new EmptyGuidRule();

        // Act
        var descriptors = rule.Describe();

        // Assert
        descriptors.Count.ShouldBe(1);
        descriptors.ShouldContain(d => d.Message == "Guid cannot be empty.");
    }
}

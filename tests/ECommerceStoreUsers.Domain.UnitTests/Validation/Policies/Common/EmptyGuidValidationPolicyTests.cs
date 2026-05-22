using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Common;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.Validation.Policies.Common;

public class EmptyGuidValidationPolicyTests
{
    [Fact]
    public async Task Validate_GuidIsEmpty_ShouldReturnError()
    {
        // Arrange
        var policy = new EmptyGuidValidationPolicy();

        // Act
        var result = await policy.Validate(Guid.Empty);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.GetValidationErrors().Count.ShouldBe(1);
        result.GetValidationErrors().ShouldContain(e => e.Name == "EmptyGuidRule");
    }

    [Fact]
    public async Task Validate_GuidIsNotEmpty_ShouldReturnNoErrors()
    {
        // Arrange
        var policy = new EmptyGuidValidationPolicy();

        // Act
        var result = await policy.Validate(Guid.NewGuid());

        // Assert
        result.IsValid.ShouldBeTrue();
        result.GetValidationErrors().Count.ShouldBe(0);
    }
}

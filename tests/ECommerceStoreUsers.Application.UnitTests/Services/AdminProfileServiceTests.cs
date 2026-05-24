using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Services.Concrete.Admins;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerceStoreUsers.Application.UnitTests.Services;

public sealed class AdminProfileServiceTests
{
    [Fact]
    public async Task CreateAdmin_WhenRequestIsValid_ShouldCreateAndReturnAdminResponse()
    {
        var request = CreateAdminRequest();
        var validationResult = new ValidationResult();
        var cancellationToken = CancellationToken.None;

        var adminRepositoryMock = new Mock<IAdminRepository>(MockBehavior.Strict);
        var adminValidationPolicyMock = new Mock<IValidationPolicy<Admin>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<AdminProfileService>>(MockBehavior.Loose);

        adminValidationPolicyMock
            .Setup(policy => policy.Validate(It.Is<Admin>(a => a.ExternalId == request.ExternalId)))
            .ReturnsAsync(validationResult);

        adminRepositoryMock
            .Setup(repo => repo.GetByExternalIdAsync(request.ExternalId, cancellationToken))
            .ReturnsAsync((Admin?)null);

        adminRepositoryMock
            .Setup(repo => repo.CreateAdmin(It.Is<Admin>(a => a.ExternalId == request.ExternalId), cancellationToken))
            .ReturnsAsync((Admin admin, CancellationToken _) => admin);

        var sut = new AdminProfileService(
            adminRepositoryMock.Object,
            adminValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        var result = await sut.CreateAdmin(request, cancellationToken);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.ExternalId, result.ExternalId);
        Assert.Equal(request.FullName, result.FullName);
        Assert.Equal(request.Email, result.Email);

        adminValidationPolicyMock.Verify(policy => policy.Validate(It.IsAny<Admin>()), Times.Once);
        adminRepositoryMock.Verify(repo => repo.GetByExternalIdAsync(request.ExternalId, cancellationToken), Times.Once);
        adminRepositoryMock.Verify(repo => repo.CreateAdmin(It.IsAny<Admin>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateAdmin_WhenAdminAlreadyExists_ShouldThrowResourceAlreadyExistsException()
    {
        var request = CreateAdminRequest();
        var validationResult = new ValidationResult();
        var cancellationToken = CancellationToken.None;
        var existingAdmin = new Admin(request.ExternalId, request.FullName, request.Email);

        var adminRepositoryMock = new Mock<IAdminRepository>(MockBehavior.Strict);
        var adminValidationPolicyMock = new Mock<IValidationPolicy<Admin>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<AdminProfileService>>(MockBehavior.Loose);

        adminValidationPolicyMock
            .Setup(policy => policy.Validate(It.Is<Admin>(a => a.ExternalId == request.ExternalId)))
            .ReturnsAsync(validationResult);

        adminRepositoryMock
            .Setup(repo => repo.GetByExternalIdAsync(request.ExternalId, cancellationToken))
            .ReturnsAsync(existingAdmin);

        var sut = new AdminProfileService(
            adminRepositoryMock.Object,
            adminValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        await Assert.ThrowsAsync<ResourceAlreadyExistsException>(() => sut.CreateAdmin(request, cancellationToken));

        adminValidationPolicyMock.Verify(policy => policy.Validate(It.IsAny<Admin>()), Times.Once);
        adminRepositoryMock.Verify(repo => repo.GetByExternalIdAsync(request.ExternalId, cancellationToken), Times.Once);
        adminRepositoryMock.Verify(repo => repo.CreateAdmin(It.IsAny<Admin>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAdmin_WhenValidationFails_ShouldThrowValidationException()
    {
        var request = CreateAdminRequest();
        var invalidResult = new ValidationResult();
        invalidResult.AddValidationError(new ValidationError
        {
            Entity = nameof(Admin),
            Name = "ExternalId",
            Message = "ExternalId is required"
        });

        var adminRepositoryMock = new Mock<IAdminRepository>(MockBehavior.Strict);
        var adminValidationPolicyMock = new Mock<IValidationPolicy<Admin>>(MockBehavior.Strict);
        var emptyGuidValidationPolicyMock = new Mock<IValidationPolicy<Guid>>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<AdminProfileService>>(MockBehavior.Loose);

        adminValidationPolicyMock
            .Setup(policy => policy.Validate(It.Is<Admin>(a => a.ExternalId == request.ExternalId)))
            .ReturnsAsync(invalidResult);

        var sut = new AdminProfileService(
            adminRepositoryMock.Object,
            adminValidationPolicyMock.Object,
            emptyGuidValidationPolicyMock.Object,
            loggerMock.Object);

        await Assert.ThrowsAsync<ValidationException>(() => sut.CreateAdmin(request, CancellationToken.None));

        adminValidationPolicyMock.Verify(policy => policy.Validate(It.IsAny<Admin>()), Times.Once);
        adminRepositoryMock.Verify(repo => repo.GetByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        adminRepositoryMock.Verify(repo => repo.CreateAdmin(It.IsAny<Admin>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static CreateAdminRequestDto CreateAdminRequest()
    {
        return new CreateAdminRequestDto
        {
            ExternalId = "admin-ext-123",
            FullName = "Jane Admin",
            Email = "jane.admin@example.com"
        };
    }
}

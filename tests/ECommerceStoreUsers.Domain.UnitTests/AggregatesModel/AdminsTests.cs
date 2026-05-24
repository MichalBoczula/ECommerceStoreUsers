using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using Shouldly;

namespace ECommerceStoreUsers.Domain.UnitTests.AggregatesModel
{
    public class AdminsTests
    {
        [Fact]
        public void Constructor_ShouldInitializeCoreProperties()
        {
            // Act
            var admin = new Admin("ext-123", "John Doe", "john@example.com");

            // Assert
            admin.Id.ShouldNotBe(Guid.Empty);
            admin.ExternalId.ShouldBe("ext-123");
            admin.FullName.ShouldBe("John Doe");
            admin.Email.ShouldBe("john@example.com");
            admin.IsActive.ShouldBeTrue();
            admin.LastLoginAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void RecordLogin_WhenActive_ShouldUpdateLastLoginAt()
        {
            // Arrange
            var admin = new Admin("ext-123", "John Doe", "john@example.com");
            var originalLoginAt = admin.LastLoginAt;

            // Act
            admin.RecordLogin();

            // Assert
            admin.LastLoginAt.ShouldBeGreaterThanOrEqualTo(originalLoginAt);
        }

        [Fact]
        public void RecordLogin_WhenInactive_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var admin = new Admin("ext-123", "John Doe", "john@example.com");
            admin.Deactivate();

            // Act
            var action = () => admin.RecordLogin();

            // Assert
            action.ShouldThrow<InvalidOperationException>()
                .Message.ShouldBe("Cannot record login for inactive admin.");
        }

        [Fact]
        public void DeactivateAndActivate_ShouldToggleIsActiveFlag()
        {
            // Arrange
            var admin = new Admin("ext-123", "John Doe", "john@example.com");

            // Act
            admin.Deactivate();
            var inactiveValue = admin.IsActive;
            admin.Activate();

            // Assert
            inactiveValue.ShouldBeFalse();
            admin.IsActive.ShouldBeTrue();
        }

        [Fact]
        public void Rehydrate_ShouldRebuildAdminWithGivenValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var lastLoginAt = DateTime.UtcNow.AddDays(-2);

            // Act
            var admin = Admin.Rehydrate(id, "ext-999", "Jane Smith", "jane@example.com", false, lastLoginAt);

            // Assert
            admin.Id.ShouldBe(id);
            admin.ExternalId.ShouldBe("ext-999");
            admin.FullName.ShouldBe("Jane Smith");
            admin.Email.ShouldBe("jane@example.com");
            admin.IsActive.ShouldBeFalse();
            admin.LastLoginAt.ShouldBe(lastLoginAt);
        }
    }
}

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Services.Abstract.Admins;
using ECommerceStoreUsers.Application.Services.Concrete.Admins;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Application
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class AdminProfileServiceBenchmarks
    {
        private IServiceProvider _serviceProvider = null!;
        private IAdminProfileService _service = null!;

        private readonly Mock<IAdminRepository> _repositoryMock = new();
        private readonly Mock<IValidationPolicy<Admin>> _adminPolicyMock = new();
        private readonly Mock<IValidationPolicy<Guid>> _guidPolicyMock = new();

        private Guid _adminId;
        private string _externalId = null!;
        private CreateAdminRequestDto _createRequestDto = null!;
        private UpdateAdminProfileRequestDto _updateRequestDto = null!;
        private Admin _domainEntity = null!;

        [GlobalSetup]
        public void Setup()
        {
            _adminId = Guid.NewGuid();
            _externalId = "entra-id|admin-999";

            _createRequestDto = AdminServiceBenchmarkDataFactory.CreateRequest();
            _updateRequestDto = AdminServiceBenchmarkDataFactory.UpdateRequest();
            _domainEntity = AdminServiceBenchmarkDataFactory.CreateDomainAdmin(_adminId, _externalId);

            _adminPolicyMock
                .Setup(x => x.Validate(It.IsAny<Admin>()))
                .ReturnsAsync(new ValidationResult());

            _guidPolicyMock
                .Setup(x => x.Validate(It.IsAny<Guid>()))
                .ReturnsAsync(new ValidationResult());

            _repositoryMock
                .Setup(x => x.CreateAdmin(It.IsAny<Admin>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Admin a, CancellationToken c) => a);

            _repositoryMock
                .Setup(x => x.UpdateAdmin(It.IsAny<Admin>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Admin a, CancellationToken c) => a);

            _repositoryMock
                .Setup(x => x.GetByExternalIdAsync(_externalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_domainEntity);

            _repositoryMock
                .Setup(x => x.GetByExternalIdAsync("entra-id|admin-unique-new", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Admin?)null);

            _repositoryMock
                .Setup(x => x.GetByIdAsync(_adminId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_domainEntity);

            var services = new ServiceCollection();

            services.AddScoped<IAdminProfileService, AdminProfileService>();

            // Register Mocks
            services.AddSingleton(_repositoryMock.Object);
            services.AddSingleton(_adminPolicyMock.Object);
            services.AddSingleton(_guidPolicyMock.Object);
            services.AddSingleton<ILogger<AdminProfileService>>(NullLogger<AdminProfileService>.Instance);

            _serviceProvider = services.BuildServiceProvider();
            _service = _serviceProvider.GetRequiredService<IAdminProfileService>();
        }

        [Benchmark(Baseline = true)]
        public async Task GetAdminByExternalId_Flow()
        {
            await _service.GetAdminByExternalId(_externalId, CancellationToken.None);
        }

        [Benchmark]
        public async Task CreateAdmin_Flow()
        {
            var volatileRequest = new CreateAdminRequestDto
            {
                ExternalId = "entra-id|admin-unique-new",
                FullName = _createRequestDto.FullName,
                Email = _createRequestDto.Email
            };

            await _service.CreateAdmin(volatileRequest, CancellationToken.None);
        }

        [Benchmark]
        public async Task UpdateAdminProfile_Flow()
        {
            await _service.UpdateAdminProfile(_adminId, _updateRequestDto, CancellationToken.None);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
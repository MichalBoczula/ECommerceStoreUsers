using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Services.Abstract.Customers;
using ECommerceStoreUsers.Application.Services.Concrete.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Application
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class CustomerServiceBenchmarks
    {
        private IServiceProvider _serviceProvider = null!;
        private ICustomerService _service = null!;

        private readonly Mock<ICustomerRepository> _repositoryMock = new();
        private readonly Mock<IValidationPolicy<Customer>> _customerPolicyMock = new();
        private readonly Mock<IValidationPolicy<IndividualData>> _individualDataPolicyMock = new();
        private readonly Mock<IValidationPolicy<CompanyData>> _companyPolicyMock = new();
        private readonly Mock<IValidationPolicy<Guid>> _guidPolicyMock = new();

        private Guid _customerId;
        private string _externalId = null!;
        private CreateCustomerRequestDto _createRequest = null!;
        private UpdateIndividualDataRequestDto _updateIndividualRequest = null!;
        private AddCompanyRequestDto _addCompanyRequest = null!;
        private UpdateCompanyRequestDto _updateCompanyRequest = null!;
        private Customer _domainEntity = null!;
        private Guid _existingCompanyId;

        [GlobalSetup]
        public void Setup()
        {
            _customerId = Guid.NewGuid();
            _externalId = "auth0|customer-999";

            _createRequest = CustomerServiceBenchmarkDataFactory.CreateRequest();
            _updateIndividualRequest = CustomerServiceBenchmarkDataFactory.UpdateIndividualRequest();
            _addCompanyRequest = CustomerServiceBenchmarkDataFactory.AddCompanyRequest();
            _updateCompanyRequest = CustomerServiceBenchmarkDataFactory.UpdateCompanyRequest();
            _domainEntity = CustomerServiceBenchmarkDataFactory.CreateDomainCustomer(_customerId, _externalId);
            _existingCompanyId = _domainEntity.Companies.First().Id;

            _customerPolicyMock
                .Setup(x => x.Validate(It.IsAny<Customer>()))
                .ReturnsAsync(new ValidationResult());

            _individualDataPolicyMock
                .Setup(x => x.Validate(It.IsAny<IndividualData>()))
                .ReturnsAsync(new ValidationResult());

            _companyPolicyMock
                .Setup(x => x.Validate(It.IsAny<CompanyData>()))
                .ReturnsAsync(new ValidationResult());

            _guidPolicyMock
                .Setup(x => x.Validate(It.IsAny<Guid>()))
                .ReturnsAsync(new ValidationResult());

            _repositoryMock
                .Setup(x => x.CreateCustomer(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer c, CancellationToken _) => c);

            _repositoryMock
                .Setup(x => x.UpdateCustomer(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer c, CancellationToken _) => c);

            _repositoryMock
                .Setup(x => x.GetByExternalIdAsync(_externalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_domainEntity);

            _repositoryMock
                .Setup(x => x.GetByExternalIdAsync("auth0|customer-unique-new", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer?)null);

            _repositoryMock
                .Setup(x => x.GetByIdAsync(_customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid _, CancellationToken _) => CustomerServiceBenchmarkDataFactory.CreateDomainCustomer(_customerId, _externalId));

            var services = new ServiceCollection();
            services.AddScoped<ICustomerService, CustomerService>();

            services.AddSingleton(_repositoryMock.Object);
            services.AddSingleton(_customerPolicyMock.Object);
            services.AddSingleton(_individualDataPolicyMock.Object);
            services.AddSingleton(_companyPolicyMock.Object);
            services.AddSingleton(_guidPolicyMock.Object);
            services.AddSingleton<ILogger<CustomerService>>(NullLogger<CustomerService>.Instance);

            _serviceProvider = services.BuildServiceProvider();
            _service = _serviceProvider.GetRequiredService<ICustomerService>();
        }

        [Benchmark(Baseline = true)]
        public async Task GetCustomerByExternalId_Flow() =>
            await _service.GetCustomerByExternalId(_externalId, CancellationToken.None);

        [Benchmark]
        public async Task CreateCustomer_Flow()
        {
            var request = CustomerServiceBenchmarkDataFactory.CreateRequest();
            request.ExternalId = "auth0|customer-unique-new";

            await _service.CreateCustomer(request, CancellationToken.None);
        }

        [Benchmark]
        public async Task UpdateIndividualData_Flow() =>
            await _service.UpdateIndividualData(_customerId, _updateIndividualRequest, CancellationToken.None);

        [Benchmark]
        public async Task AddCompany_Flow() =>
            await _service.AddCompany(_customerId, _addCompanyRequest, CancellationToken.None);

        [Benchmark]
        public async Task UpdateCompany_Flow() =>
            await _service.UpdateCompany(_customerId, _existingCompanyId, _updateCompanyRequest, CancellationToken.None);

        [GlobalCleanup]
        public void Cleanup()
        {
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }
    }
}

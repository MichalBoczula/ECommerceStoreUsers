using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using Microsoft.Extensions.DependencyInjection;
using ECommerceStoreUsers.Domain.Validation.Concrete.Policies.Employees.Admins;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Domain.Admins
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class AdminValidationPolicyBenchmarks
    {
        private IServiceProvider _serviceProvider = null!;
        private IServiceScope _serviceScope = null!;
        private IValidationPolicy<Admin> _policy = null!;
        private Admin _validEntity = null!;
        private Admin _invalidEmailEntity = null!;
        private Admin _allInvalidEntity = null!;

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();

            services.AddScoped<IValidationPolicy<Admin>, AdminValidationPolicy>();

            _serviceProvider = services.BuildServiceProvider();
            _serviceScope = _serviceProvider.CreateScope();

            _policy = _serviceScope.ServiceProvider.GetRequiredService<IValidationPolicy<Admin>>();

            _validEntity = AdminValidationDataFactory.CreateValid();
            _invalidEmailEntity = AdminValidationDataFactory.CreateInvalidEmail();
            _allInvalidEntity = AdminValidationDataFactory.CreateAllInvalid();
        }

        [Benchmark(Baseline = true)]
        public async Task Validate_Success_HappyPath()
        {
            await _policy.Validate(_validEntity);
        }

        [Benchmark]
        public async Task Validate_Failure_SingleError()
        {
            await _policy.Validate(_invalidEmailEntity);
        }

        [Benchmark]
        public async Task Validate_Failure_MultipleErrors()
        {
            await _policy.Validate(_allInvalidEntity);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _serviceScope.Dispose();
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}

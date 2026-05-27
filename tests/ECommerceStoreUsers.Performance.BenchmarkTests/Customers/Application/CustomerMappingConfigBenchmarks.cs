using BenchmarkDotNet.Attributes;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Application.Common;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Application
{
    [MemoryDiagnoser]
    public class CustomerMappingConfigBenchmarks
    {
        private CreateCustomerRequestDto _request = null!;
        private Customer _customer = null!;

        [GlobalSetup]
        public void Setup()
        {
            _request = CustomerServiceBenchmarkDataFactory.CreateRequest();
            _customer = CustomerServiceBenchmarkDataFactory.CreateDomainCustomer(Guid.NewGuid(), _request.ExternalId);
        }

        [Benchmark]
        public object MapToDomain()
        {
            return MappingConfig.MapToDomain(_request);
        }

        [Benchmark]
        public object MapToResponse()
        {
            return MappingConfig.MapToResponse(_customer);
        }
    }
}

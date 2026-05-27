using BenchmarkDotNet.Attributes;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Persistance.Customers;
using ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Infrastructure.Common;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Infrastructure
{
    [MemoryDiagnoser]
    public class CustomerMappingBenchmarks
    {
        private CustomerDocument _document = null!;
        private Customer _domain = null!;

        [GlobalSetup]
        public void Setup()
        {
            _document = CustomerDocumentBenchmarkDataFactory.Create();
            _domain = CustomerMapping.MapToDomain(_document);
        }

        [Benchmark]
        public Customer MapDocumentToDomain()
        {
            return CustomerMapping.MapToDomain(_document);
        }

        [Benchmark]
        public object MapDomainToDocument()
        {
            return CustomerMapping.MapToDocument(_domain);
        }
    }
}

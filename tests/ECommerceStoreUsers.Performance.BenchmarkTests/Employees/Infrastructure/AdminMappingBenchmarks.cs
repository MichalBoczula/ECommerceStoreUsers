using BenchmarkDotNet.Attributes;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Persistance.Admins;
using ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Infrastructure.Common;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Infrastructure
{
    [MemoryDiagnoser]
    public class AdminMappingBenchmarks
    {
        private AdminDocument _document = null!;
        private Admin _domain = null!;

        [GlobalSetup]
        public void Setup()
        {
            _document = AdminDocumentBenchmarkDataFactory.Create();
            _domain = AdminMapping.MapToDomain(_document);
        }

        [Benchmark]
        public Admin MapDocumentToDomain()
        {
            return AdminMapping.MapToDomain(_document);
        }

        [Benchmark]
        public object MapDomainToDocument()
        {
            return AdminMapping.MapToDocument(_domain);
        }
    }
}

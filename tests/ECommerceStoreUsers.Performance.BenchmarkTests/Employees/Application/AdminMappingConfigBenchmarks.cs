using BenchmarkDotNet.Attributes;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Mapping;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Application.Common;

namespace ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Application
{
    [MemoryDiagnoser]
    public class AdminMappingConfigBenchmarks
    {
        private CreateAdminRequestDto _request = null!;
        private Admin _admin = null!;

        [GlobalSetup]
        public void Setup()
        {
            _request = AdminServiceBenchmarkDataFactory.CreateRequest();
            _admin = AdminServiceBenchmarkDataFactory.CreateDomainAdmin(Guid.NewGuid(), _request.ExternalId);
        }

        [Benchmark]
        public object MapToDomain()
        {
            return MappingConfig.MapToDomain(_request);
        }

        [Benchmark]
        public object MapToResponse()
        {
            return MappingConfig.MapToResponse(_admin);
        }
    }
}
using BenchmarkDotNet.Running;
using ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Domain.Admins;
using ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Infrastructure;
using ECommerceStoreUsers.Performance.BenchmarkTests.Customers.Infrastructure;

namespace ECommerceStoreUsers.Performance.BenchmarkTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // ==========================================
            // 1. Employees / Admins
            // ==========================================
            BenchmarkRunner.Run<AdminValidationPolicyBenchmarks>();
            BenchmarkRunner.Run<AdminMappingBenchmarks>();
            BenchmarkRunner.Run<AdminRepositoryBenchmarks>();

            // ==========================================
            // 2. Customers
            // ==========================================
            BenchmarkRunner.Run<CustomerMappingBenchmarks>();
        }
    }
}

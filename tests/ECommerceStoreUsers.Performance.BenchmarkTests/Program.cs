using BenchmarkDotNet.Running;
using ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Domain;
using ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Infrastructure;

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
        }
    }
}

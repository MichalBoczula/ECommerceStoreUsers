using BenchmarkDotNet.Running;
using ECommerceStoreUsers.Performance.BenchmarkTests.Employees.Domain;

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

            // ==========================================
            // 2. Customers
            // ==========================================
        }
    }
}

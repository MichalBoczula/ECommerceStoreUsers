using ECommerceStoreUsers.API;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ECommerceStoreUsers.AcceptanceTests
{
    public class ApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        Task IAsyncLifetime.DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}

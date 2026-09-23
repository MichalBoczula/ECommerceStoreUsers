using Reqnroll;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common
{
    [Binding]
    public sealed class TestHooks
    {
        private readonly ScenarioApiContext _apiContext;
        private ApplicationFactory? _factory;
        private string? _databaseName;

        public TestHooks(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [BeforeTestRun]
        public static Task BeforeTestRun()
        {
            return AcceptanceMongoDb.StartAsync();
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            _databaseName = $"acceptance-{Guid.NewGuid():N}";
            try
            {
                _factory = new ApplicationFactory(
                    AcceptanceMongoDb.ConnectionString,
                    _databaseName);

                _apiContext.DatabaseName = _databaseName;
                _apiContext.Factory = _factory;
                _apiContext.HttpClient = _factory.CreateClient();
            }
            catch
            {
                await CleanupAsync();
                throw;
            }
        }

        [AfterScenario]
        public Task AfterScenario() => CleanupAsync();

        private async Task CleanupAsync()
        {
            try
            {
                try
                {
                    _apiContext.Response?.Dispose();
                    _apiContext.HttpClient?.Dispose();
                }
                finally
                {
                    if (_factory is not null)
                    {
                        await _factory.DisposeAsync();
                    }
                }
            }
            finally
            {
                _factory = null;
                _apiContext.Response = null;
                _apiContext.HttpClient = default!;
                _apiContext.Factory = default!;
                _apiContext.DatabaseName = string.Empty;
                if (_databaseName is not null)
                {
                    var databaseName = _databaseName;
                    _databaseName = null;
                    await AcceptanceMongoDb.DropDatabaseAsync(databaseName);
                }
            }
        }

        [AfterTestRun]
        public static Task AfterTestRun()
        {
            return AcceptanceMongoDb.DisposeAsync();
        }
    }
}

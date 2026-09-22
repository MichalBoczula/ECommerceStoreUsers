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
        public void BeforeScenario()
        {
            _databaseName = $"acceptance-{Guid.NewGuid():N}";
            _factory = new ApplicationFactory(
                AcceptanceMongoDb.ConnectionString,
                _databaseName);

            _apiContext.Factory = _factory;
            _apiContext.HttpClient = _factory.CreateClient();
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            try
            {
                _apiContext.Response?.Dispose();
                _apiContext.HttpClient?.Dispose();

                if (_factory is not null)
                {
                    await _factory.DisposeAsync();
                }
            }
            finally
            {
                if (_databaseName is not null)
                {
                    await AcceptanceMongoDb.DropDatabaseAsync(_databaseName);
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

using Reqnroll;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common
{
    [Binding]
    public sealed class TestHooks
    {
        private readonly ScenarioApiContext _apiContext;
        private ApplicationFactory? _factory;
        private JsonDocument? _openApi;
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
                using (var swaggerClient = _factory.CreateClient())
                    _openApi = JsonDocument.Parse(await swaggerClient.GetStringAsync("/swagger/v1/swagger.json"));
                _apiContext.OpenApiDocument = _openApi;
                _apiContext.HttpClient = _factory.CreateDefaultClient(new OpenApiResponseHandler(_openApi));
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
                    _openApi?.Dispose();
                    _openApi = null;
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
                _apiContext.OpenApiDocument = default!;
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

using Allure.Net.Commons;
using Reqnroll;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common
{
    [Binding]
    public sealed class TestHooks
    {
        private readonly ScenarioApiContext _apiContext;
        private readonly ScenarioContext _scenarioContext;
        private readonly FeatureContext _featureContext;
        private ApplicationFactory? _factory;
        private JsonDocument? _openApi;
        private string? _databaseName;

        public TestHooks(ScenarioApiContext apiContext, ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            _apiContext = apiContext;
            _scenarioContext = scenarioContext;
            _featureContext = featureContext;
        }

        [BeforeTestRun]
        public static Task BeforeTestRun()
        {
            // The contract command filters to the read-only export test, whose
            // host deliberately removes Mongo startup. Real suites still use Mongo.
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAPI_EXPORT_PATH")))
            {
                return Task.CompletedTask;
            }

            return AcceptanceMongoDb.StartAsync();
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            _apiContext.InitializeEvidence(_scenarioContext.ScenarioInfo, _featureContext.FeatureInfo.Title);
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
                _apiContext.HttpClient = _apiContext.CreateClient(_factory);
            }
            catch
            {
                await CleanupAsync();
                throw;
            }
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            try
            {
                try
                {
                    if (_scenarioContext.TestError is null) _apiContext.CompleteDocumentation();
                }
                finally { _apiContext.FlushEvidence(); }
            }
            finally { await CleanupAsync(); }
        }

        [BeforeStep]
        public void BeforeStep()
        {
            _apiContext.BeginStep(_scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString());
            _apiContext.PublishEvidenceIdentity();
            var description = _scenarioContext.ScenarioInfo.Description;
            if (string.IsNullOrWhiteSpace(description))
                throw new InvalidOperationException("A readable scenario description is required for BDD evidence.");
            AllureApi.SetDescription(description);
        }

        [AfterStep]
        public void AfterStep() => _apiContext.FlushEvidence();

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


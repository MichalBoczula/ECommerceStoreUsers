using ECommerceStoreInvoice.API.Configuration.Common;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common
{
    [Binding]
    public sealed class UnexpectedServerErrorsStepDefinitions : IDisposable
    {
        private const string SensitiveDetail = "INTERNAL_FAILURE_MARKER_DO_NOT_EXPOSE";

        private readonly ScenarioApiContext _apiContext;
        private WebApplicationFactory<ECommerceStoreUsers.API.Program>? _factory;
        private HttpClient? _client;
        private IFailureProbe? _failureProbe;
        private string? _area;
        private string? _requestPath;

        public UnexpectedServerErrorsStepDefinitions(ScenarioApiContext apiContext)
        {
            _apiContext = apiContext;
        }

        [Given("the {string} read dependency fails unexpectedly")]
        public void GivenTheReadDependencyFailsUnexpectedly(string area)
        {
            _area = area;

            _factory = _apiContext.Factory.WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                {
                    switch (area)
                    {
                        case "customer":
                            var customerRepository = new FailingCustomerRepository();
                            _failureProbe = customerRepository;
                            services.RemoveAll<ICustomerRepository>();
                            services.AddSingleton<ICustomerRepository>(customerRepository);
                            break;

                        case "admin":
                            var adminRepository = new FailingAdminRepository();
                            _failureProbe = adminRepository;
                            services.RemoveAll<IAdminRepository>();
                            services.AddSingleton<IAdminRepository>(adminRepository);
                            break;

                        case "favorites":
                            var favoriteRepository = new FailingFavoriteRepository();
                            _failureProbe = favoriteRepository;
                            services.RemoveAll<IFavoriteRepository>();
                            services.AddSingleton<IFavoriteRepository>(favoriteRepository);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown server-error test area.");
                    }
                }));

            _client = _factory.CreateClient();
        }

        [When("I request the failing {string} endpoint")]
        public async Task WhenIRequestTheFailingEndpoint(string area)
        {
            _client.ShouldNotBeNull();
            area.ShouldBe(_area);

            _requestPath = area switch
            {
                "customer" => "/customers/external/server-error-customer",
                "admin" => "/admins/external/server-error-admin",
                "favorites" => "/favorites/clients/11111111-1111-1111-1111-111111111111",
                _ => throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown server-error test area.")
            };

            _apiContext.Response = await _client.GetAsync(_requestPath);
        }

        [Then("a safe custom server error is returned")]
        public async Task ThenASafeCustomServerErrorIsReturned(Table table)
        {
            var expected = table.Rows.ToDictionary(row => row["Field"], row => row["Value"]);

            _apiContext.Response.ShouldNotBeNull();
            _failureProbe.ShouldNotBeNull();
            _requestPath.ShouldNotBeNull();

            _failureProbe.Calls.ShouldBe(1);
            _apiContext.Response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            _apiContext.Response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

            var body = await _apiContext.Response.Content.ReadAsStringAsync();
            AllureJson.AttachRawJson("Safe internal server error response", body);

            var problem = JsonSerializer.Deserialize<InternalServerErrorProblemDetails>(body, _apiContext.JsonOptions);
            problem.ShouldNotBeNull();
            problem.Status.ShouldBe(int.Parse(expected["StatusCode"], CultureInfo.InvariantCulture));
            problem.Title.ShouldBe(expected["Title"]);
            problem.Detail.ShouldBe(expected["Detail"]);
            problem.Type.ShouldBe("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1");
            problem.Instance.ShouldBe(_requestPath);
            problem.TraceId.ShouldNotBeNullOrWhiteSpace();

            body.ShouldNotContain(SensitiveDetail);
            body.ShouldNotContain(nameof(InvalidOperationException));
            body.ShouldNotContain(nameof(FailingCustomerRepository));
            body.ShouldNotContain(nameof(FailingAdminRepository));
            body.ShouldNotContain(nameof(FailingFavoriteRepository));
            body.ShouldNotContain("stackTrace");
            body.ShouldNotContain("exception");
        }

        public void Dispose()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        private interface IFailureProbe
        {
            int Calls { get; }
        }

        private abstract class FailureProbe : IFailureProbe
        {
            public int Calls { get; private set; }

            protected Task<T> Fail<T>()
            {
                Calls++;
                return Task.FromException<T>(new InvalidOperationException(SensitiveDetail));
            }
        }

        private sealed class FailingCustomerRepository : FailureProbe, ICustomerRepository
        {
            public Task<Customer?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
                => Fail<Customer?>();

            public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public Task<Customer> CreateCustomer(Customer customer, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public Task<Customer> UpdateCustomer(Customer customer, CancellationToken cancellationToken)
                => throw new NotSupportedException();
        }

        private sealed class FailingAdminRepository : FailureProbe, IAdminRepository
        {
            public Task<Admin?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
                => Fail<Admin?>();

            public Task<Admin?> GetByIdAsync(Guid adminId, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public Task<Admin> CreateAdmin(Admin admin, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public Task<Admin> UpdateAdmin(Admin admin, CancellationToken cancellationToken)
                => throw new NotSupportedException();
        }

        private sealed class FailingFavoriteRepository : FailureProbe, IFavoriteRepository
        {
            public Task<IReadOnlyList<Favorite>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
                => Fail<IReadOnlyList<Favorite>>();

            public Task<Favorite?> GetByClientAndProductIdAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default)
                => throw new NotSupportedException();

            public Task<bool> ExistsAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default)
                => throw new NotSupportedException();

            public Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
                => throw new NotSupportedException();

            public Task DeleteAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default)
                => throw new NotSupportedException();

            public Task DeleteAllByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
                => throw new NotSupportedException();
        }
    }
}

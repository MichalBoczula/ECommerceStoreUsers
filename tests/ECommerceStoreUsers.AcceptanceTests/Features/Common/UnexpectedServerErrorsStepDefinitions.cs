using ECommerceStoreUsers.API.Configuration.Common;
using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Services.Abstract.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reqnroll;
using Shouldly;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
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

        [Given("the {string} dependency fails unexpectedly")]
        public void GivenTheDependencyFailsUnexpectedly(string area)
        {
            _area = area;

            _factory = _apiContext.Factory.WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                {
                    switch (area)
                    {
                        case "customer":
                            ReplaceCustomerRepository(services);
                            break;

                        case "admin":
                            ReplaceAdminRepository(services);
                            break;

                        case "favorites":
                        case "favorite-add":
                        case "favorite-remove":
                        case "favorite-clear":
                            ReplaceFavoriteRepository(services, area);
                            break;

                        case "documentation-flows":
                            var flowService = new FailingFavoriteFlowDescriptorService();
                            _failureProbe = flowService;
                            services.RemoveAll<IFavoriteFlowDescriptorService>();
                            services.AddSingleton<IFavoriteFlowDescriptorService>(flowService);
                            break;

                        case "documentation-validations":
                            var validationProvider = new FailingValidationDescriptorProvider();
                            _failureProbe = validationProvider;
                            services.RemoveAll<IValidationPolicyDescriptorProvider>();
                            services.AddSingleton<IValidationPolicyDescriptorProvider>(validationProvider);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(
                                nameof(area),
                                area,
                                "Unknown server-error test area.");
                    }
                }));

            _client = _factory.CreateClient();
        }

        [When("I request the failing {string} endpoint")]
        public async Task WhenIRequestTheFailingEndpoint(string area)
        {
            _client.ShouldNotBeNull();
            area.ShouldBe(_area);

            const string clientId = "11111111-1111-1111-1111-111111111111";
            const string productId = "22222222-2222-2222-2222-222222222222";

            _requestPath = area switch
            {
                "customer" => "/customers/external/server-error-customer",
                "admin" => "/admins/external/server-error-admin",
                "favorites" => $"/favorites/clients/{clientId}",
                "favorite-add" => $"/favorites/clients/{clientId}",
                "favorite-remove" => $"/favorites/clients/{clientId}/products/{productId}",
                "favorite-clear" => $"/favorites/clients/{clientId}",
                "documentation-flows" => "/users-documentation/flows",
                "documentation-validations" => "/users-documentation/validations",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(area),
                    area,
                    "Unknown server-error test area.")
            };

            _apiContext.Response = area switch
            {
                "favorite-add" => await _client.PostAsJsonAsync(
                    _requestPath,
                    new { ProductId = Guid.Parse(productId) }),
                "favorite-remove" or "favorite-clear" => await _client.DeleteAsync(_requestPath),
                _ => await _client.GetAsync(_requestPath)
            };
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

            var problem = JsonSerializer.Deserialize<InternalServerErrorProblemDetails>(
                body,
                _apiContext.JsonOptions);
            problem.ShouldNotBeNull();
            problem.Status.ShouldBe(int.Parse(expected["StatusCode"], CultureInfo.InvariantCulture));
            problem.Title.ShouldBe(expected["Title"]);
            problem.Detail.ShouldBe(expected["Detail"]);
            problem.Type.ShouldBe("https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1");
            problem.Instance.ShouldBe(_requestPath);
            problem.TraceId.ShouldNotBeNullOrWhiteSpace();

            body.ShouldNotContain(SensitiveDetail);
            body.ShouldNotContain(nameof(InvalidOperationException));
            body.ShouldNotContain("stackTrace");
            body.ShouldNotContain("exception");
        }

        public void Dispose()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        private void ReplaceCustomerRepository(IServiceCollection services)
        {
            var repository = new FailingCustomerRepository();
            _failureProbe = repository;
            services.RemoveAll<ICustomerRepository>();
            services.AddSingleton<ICustomerRepository>(repository);
        }

        private void ReplaceAdminRepository(IServiceCollection services)
        {
            var repository = new FailingAdminRepository();
            _failureProbe = repository;
            services.RemoveAll<IAdminRepository>();
            services.AddSingleton<IAdminRepository>(repository);
        }

        private void ReplaceFavoriteRepository(IServiceCollection services, string operation)
        {
            var repository = new FailingFavoriteRepository(operation);
            _failureProbe = repository;
            services.RemoveAll<IFavoriteRepository>();
            services.AddSingleton<IFavoriteRepository>(repository);
        }

        private interface IFailureProbe
        {
            int Calls { get; }
        }

        private abstract class FailureProbe : IFailureProbe
        {
            public int Calls { get; private set; }

            protected Task Fail()
            {
                Calls++;
                return Task.FromException(new InvalidOperationException(SensitiveDetail));
            }

            protected Task<T> Fail<T>()
            {
                Calls++;
                return Task.FromException<T>(new InvalidOperationException(SensitiveDetail));
            }

            protected T FailSynchronously<T>()
            {
                Calls++;
                throw new InvalidOperationException(SensitiveDetail);
            }
        }

        private sealed class FailingCustomerRepository : FailureProbe, ICustomerRepository
        {
            public Task<Customer?> GetByExternalIdAsync(
                string externalId,
                CancellationToken cancellationToken)
                => Fail<Customer?>();

            public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public Task<Customer> CreateCustomer(
                Customer customer,
                CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public Task<Customer> UpdateCustomer(
                Customer customer,
                CancellationToken cancellationToken)
                => throw new NotSupportedException();
        }

        private sealed class FailingAdminRepository : FailureProbe, IAdminRepository
        {
            public Task<Admin?> GetByExternalIdAsync(
                string externalId,
                CancellationToken cancellationToken)
                => Fail<Admin?>();

            public Task<Admin?> GetByIdAsync(Guid adminId, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public Task<Admin> CreateAdmin(Admin admin, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public Task<Admin> UpdateAdmin(Admin admin, CancellationToken cancellationToken)
                => throw new NotSupportedException();
        }

        private sealed class FailingFavoriteRepository(
            string operation) : FailureProbe, IFavoriteRepository
        {
            public Task<IReadOnlyList<Favorite>> GetByClientIdAsync(
                Guid clientId,
                CancellationToken cancellationToken = default)
                => operation == "favorites"
                    ? Fail<IReadOnlyList<Favorite>>()
                    : throw new NotSupportedException();

            public Task<Favorite?> GetByClientAndProductIdAsync(
                Guid clientId,
                Guid productId,
                CancellationToken cancellationToken = default)
                => operation == "favorite-remove"
                    ? Fail<Favorite?>()
                    : throw new NotSupportedException();

            public Task<bool> ExistsAsync(
                Guid clientId,
                Guid productId,
                CancellationToken cancellationToken = default)
                => operation == "favorite-add"
                    ? Fail<bool>()
                    : throw new NotSupportedException();

            public Task AddAsync(
                Favorite favorite,
                CancellationToken cancellationToken = default)
                => throw new NotSupportedException();

            public Task DeleteAsync(
                Guid clientId,
                Guid productId,
                CancellationToken cancellationToken = default)
                => throw new NotSupportedException();

            public Task DeleteAllByClientIdAsync(
                Guid clientId,
                CancellationToken cancellationToken = default)
                => operation == "favorite-clear"
                    ? Fail()
                    : throw new NotSupportedException();
        }

        private sealed class FailingFavoriteFlowDescriptorService :
            FailureProbe,
            IFavoriteFlowDescriptorService
        {
            public FlowDescriptor GetGetFavoritesByClientIdDescriptor()
                => FailSynchronously<FlowDescriptor>();

            public FlowDescriptor GetAddProductToFavoritesDescriptor()
                => throw new NotSupportedException();

            public FlowDescriptor GetRemoveProductFromFavoritesDescriptor()
                => throw new NotSupportedException();

            public FlowDescriptor GetClearClientFavoritesDescriptor()
                => throw new NotSupportedException();
        }

        private sealed class FailingValidationDescriptorProvider :
            FailureProbe,
            IValidationPolicyDescriptorProvider
        {
            public ValidationPolicyDescriptor Describe()
                => FailSynchronously<ValidationPolicyDescriptor>();
        }
    }
}

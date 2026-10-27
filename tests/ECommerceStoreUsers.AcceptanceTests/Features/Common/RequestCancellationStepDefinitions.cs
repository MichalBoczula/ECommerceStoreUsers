using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reqnroll;
using Shouldly;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class RequestCancellationStepDefinitions(ScenarioApiContext context, FrameworkFailureState state) : IDisposable
{
    private readonly TaskCompletionSource _entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _cancelled = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private WebApplicationFactory<ECommerceStoreUsers.API.Program>? _factory;
    private string _operation = string.Empty;
    private string _path = string.Empty;
    private string _method = string.Empty;
    private object? _body;
    private int _entries;
    private bool _serverCanCancel;
    private bool _serverCancelled;
    private Exception? _clientError;

    [Given("the {string} write waits for request cancellation")]
    public void GivenWaitingWrite(string operation)
    {
        _operation = operation;
        var customerId = state.CustomerId;
        var address = new AddressRequestDto { PostalCode = "00-001", City = "Warsaw", Street = "Cancellation Street", BuildingNumber = "10", ApartmentNumber = null };
        var individual = new IndividualDataRequestDto
        {
            FirstName = "Attempt",
            LastName = "Person",
            Email = "attempt@example.com",
            Phone = "123456789",
            BillingAddress = address,
            ShippingAddress = address
        };
        var company = new AddCompanyRequestDto { TaxId = "9876543210", CompanyName = "Attempt Company", BillingAddress = address, ShippingAddress = address };
        (_method, _path, _body) = operation switch
        {
            "CreateCustomer" => ("POST", "/customers", (object)new CreateCustomerRequestDto { ExternalId = "cancel-create-customer", Individual = individual }),
            "UpdateIndividualData" => ("PUT", $"/customers/{customerId}/individual", new UpdateIndividualDataRequestDto { Individual = individual }),
            "AddCompany" => ("POST", $"/customers/{customerId}/companies", company),
            "UpdateCompany" => ("PUT", $"/customers/{customerId}/companies/{state.CompanyId}", new UpdateCompanyRequestDto
            {
                TaxId = company.TaxId,
                CompanyName = company.CompanyName,
                BillingAddress = address,
                ShippingAddress = address
            }),
            "CreateAdmin" => ("POST", "/admins", new CreateAdminRequestDto { ExternalId = "cancel-create-admin", FullName = "Attempt Admin", Email = "attempt@example.com" }),
            "UpdateAdminProfile" => ("PUT", $"/admins/{state.AdminId}", new UpdateAdminProfileRequestDto { FullName = "Attempt Admin", Email = "attempt@example.com" }),
            "AddProductToFavorites" => ("POST", "/favorites/clients/77777777-7777-7777-7777-777777777777", new { ProductId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") }),
            "RemoveProductFromFavorites" => ("DELETE", "/favorites/clients/77777777-7777-7777-7777-777777777777/products/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", null),
            "ClearClientFavorites" => ("DELETE", "/favorites/clients/77777777-7777-7777-7777-777777777777", null),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
        context.RecordEvidence("Technical cancellation request", new { operation, method = _method, path = _path, body = _body, gate = "Repository entry before inner persistence write" });
        _factory = context.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICustomerRepository>();
            services.RemoveAll<IAdminRepository>();
            services.RemoveAll<IFavoriteRepository>();
            services.AddScoped<ICustomerRepository>(provider => new CustomerGate(new CustomerRepository(provider.GetRequiredService<MongoDbContext>()), WaitForCancellation));
            services.AddScoped<IAdminRepository>(provider => new AdminGate(new AdminRepository(provider.GetRequiredService<MongoDbContext>()), WaitForCancellation));
            services.AddScoped<IFavoriteRepository>(provider => new FavoriteGate(new FavoriteRepository(provider.GetRequiredService<MongoDbContext>()), WaitForCancellation));
        }));
        context.HttpClient.Dispose();
        context.HttpClient = context.CreateClient(_factory);
    }

    private async Task WaitForCancellation(CancellationToken token)
    {
        Interlocked.Increment(ref _entries);
        _serverCanCancel = token.CanBeCanceled;
        _entered.TrySetResult();
        try { await Task.Delay(Timeout.InfiniteTimeSpan, token); }
        finally
        {
            _serverCancelled = token.IsCancellationRequested;
            _cancelled.TrySetResult();
        }
    }

    [When("I cancel the client request after its repository entry is reached")]
    public async Task WhenCancel()
    {
        using var cancellation = new CancellationTokenSource();
        using var request = new HttpRequestMessage(new HttpMethod(_method), _path);
        if (_body is not null)
            request.Content = JsonContent.Create(_body, options: context.JsonOptions);
        var sending = context.HttpClient.SendAsync(request, cancellation.Token);
        try
        {
            await _entered.Task.WaitAsync(TimeSpan.FromSeconds(15));
            cancellation.Cancel();
            try { context.Response = await sending; }
            catch (OperationCanceledException error) { _clientError = error; }
            await _cancelled.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally { cancellation.Cancel(); }
    }

    [Then("cancellation has these observed values")]
    public void ThenCancellation(Table table)
    {
        var expected = table.Rows.ToDictionary(row => row["Field"],
        row => row["Value"]);
        context.RecordEvidence("Technical cancellation expectations", expected);
        expected.ShouldBe(new Dictionary<string, string>
        {
            ["RepositoryEntries"] = "1",
            ["ServerTokenCanCancel"] = "true",
            ["ServerTokenCancelled"] = "true",
            ["ClientCancelled"] = "true",
            ["ResponseObserved"] = "false",
            ["InnerWriteInvoked"] = "false",
            ["WriteEffect"] = "NoWrite"
        });
        _entries.ShouldBe(1);
        _serverCanCancel.ShouldBeTrue();
        _serverCancelled.ShouldBeTrue();
        (_clientError is OperationCanceledException).ShouldBeTrue();
        context.Response.ShouldBeNull();
        context.RecordEvidence("Technical cancellation verified", new
        {
            operation = _operation,
            repositoryEntries = _entries,
            serverTokenCanCancel = _serverCanCancel,
            serverTokenCancelled = _serverCancelled,
            clientCancelled = true,
            responseObserved = false,
            innerWriteInvoked = false,
            writeEffect = "NoWrite",
            clientExceptionType = _clientError!.GetType().Name,
            scope = "Before inner write; not an in-transaction rollback test"
        });
    }

    public void Dispose() => _factory?.Dispose();

    private static async Task<T> CancelBeforeWrite<T>(Func<CancellationToken, Task> gate, CancellationToken token)
    {
        await gate(token);
        throw new InvalidOperationException("Cancellation gate unexpectedly completed without cancellation.");
    }

    private sealed class CustomerGate(CustomerRepository inner, Func<CancellationToken, Task> gate) : ICustomerRepository
    {
        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken token) => inner.GetByIdAsync(id, token);
        public Task<Customer?> GetByExternalIdAsync(string id, CancellationToken token) => inner.GetByExternalIdAsync(id, token);
        public Task<Customer> CreateCustomer(Customer customer, CancellationToken token) => CancelBeforeWrite<Customer>(gate, token);
        public Task<Customer> UpdateCustomer(Customer customer, CancellationToken token) => CancelBeforeWrite<Customer>(gate, token);
    }

    private sealed class AdminGate(AdminRepository inner, Func<CancellationToken, Task> gate) : IAdminRepository
    {
        public Task<Admin?> GetByIdAsync(Guid id, CancellationToken token) => inner.GetByIdAsync(id, token);
        public Task<Admin?> GetByExternalIdAsync(string id, CancellationToken token) => inner.GetByExternalIdAsync(id, token);
        public Task<Admin> CreateAdmin(Admin admin, CancellationToken token) => CancelBeforeWrite<Admin>(gate, token);
        public Task<Admin> UpdateAdmin(Admin admin, CancellationToken token) => CancelBeforeWrite<Admin>(gate, token);
    }

    private sealed class FavoriteGate(FavoriteRepository inner, Func<CancellationToken, Task> gate) : IFavoriteRepository
    {
        public Task<IReadOnlyList<Favorite>> GetByClientIdAsync(Guid id, CancellationToken token = default) => inner.GetByClientIdAsync(id, token);
        public Task<Favorite?> GetByClientAndProductIdAsync(Guid clientId, Guid productId, CancellationToken token = default) => inner.GetByClientAndProductIdAsync(clientId, productId, token);
        public Task<bool> ExistsAsync(Guid clientId, Guid productId, CancellationToken token = default) => inner.ExistsAsync(clientId, productId, token);
        public Task AddAsync(Favorite favorite, CancellationToken token = default) => gate(token);
        public Task<bool> DeleteAsync(Guid clientId, Guid productId, CancellationToken token = default) => CancelBeforeWrite<bool>(gate, token);
        public Task DeleteAllByClientIdAsync(Guid clientId, CancellationToken token = default) => gate(token);
    }
}

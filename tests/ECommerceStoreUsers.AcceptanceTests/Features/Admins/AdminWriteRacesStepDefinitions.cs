using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using ECommerceStoreUsers.Application.Common.ResponsesDto.Admins;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Reqnroll;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins;

[Binding]
public sealed class AdminWriteRacesStepDefinitions(ScenarioApiContext context) : IDisposable
{
    private WebApplicationFactory<ECommerceStoreUsers.API.Program>? _factory;
    private Guid _id;
    private Race _race;

    [Given("an admin update loses a concurrent change race")]
    public Task GivenChangeRace() => Arrange(Race.Change);

    [Given("an admin update loses a concurrent delete race")]
    public Task GivenDeleteRace() => Arrange(Race.Delete);

    [Given("an admin profile is unchanged on update")]
    public Task GivenUnchangedProfile() => Arrange(Race.None);

    private async Task Arrange(Race race)
    {
        _race = race;
        using var response = await context.HttpClient.PostAsJsonAsync("/admins", new CreateAdminRequestDto
        {
            ExternalId = $"admin-race-{Guid.NewGuid():N}",
            FullName = "Original Admin",
            Email = "original@example.com"
        }, context.JsonOptions);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _id = (await response.Content.ReadFromJsonAsync<AdminResponseDto>(context.JsonOptions))!.Id;

        if (race == Race.None)
            return;

        _factory = context.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAdminRepository>();
            services.AddScoped<IAdminRepository>(provider =>
                new RacingRepository(new AdminRepository(provider.GetRequiredService<MongoDbContext>()),
                    provider.GetRequiredService<MongoDbContext>(), race));
        }));
        context.HttpClient.Dispose();
        context.HttpClient = _factory.CreateClient();
    }

    [When("the raced admin profile update is submitted")]
    public async Task WhenUpdate()
    {
        context.Response = await context.HttpClient.PutAsJsonAsync($"/admins/{_id}", new UpdateAdminProfileRequestDto
        {
            FullName = _race == Race.None ? "Original Admin" : "Attempt Admin",
            Email = _race == Race.None ? "original@example.com" : "attempt@example.com"
        }, context.JsonOptions);
    }

    [Then("the raced admin update returns {int} with consistent history")]
    public async Task ThenStatusAndHistory(int status)
    {
        context.Response.ShouldNotBeNull();
        context.Response.StatusCode.ShouldBe((HttpStatusCode)status);
        if (status != 200)
        {
            context.Response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
            var body = await context.Response.Content.ReadFromJsonAsync<JsonElement>(context.JsonOptions);
            body.GetProperty("code").GetString().ShouldBe(_race == Race.Delete
                ? "resource_not_found" : "concurrency_conflict");
        }
        else
        {
            var body = await context.Response.Content.ReadFromJsonAsync<AdminResponseDto>(context.JsonOptions);
            body.ShouldNotBeNull();
            body.FullName.ShouldBe("Original Admin");
        }

        var mongo = context.Factory.Services.GetRequiredService<MongoDbContext>();
        (await mongo.AdminsHistory.CountDocumentsAsync(x => x.AdminId == _id))
            .ShouldBe(_race == Race.Change ? 2 : 1);
        var persisted = await mongo.Admins.Find(x => x.Id == _id).FirstOrDefaultAsync();
        if (_race == Race.Delete)
            persisted.ShouldBeNull();
        else
        {
            persisted.ShouldNotBeNull();
            persisted.FullName.ShouldBe(_race == Race.Change ? "Winner Admin" : "Original Admin");
            persisted.Version.ShouldBe(_race == Race.Change ? 1 : 0);
        }
    }

    public void Dispose() => _factory?.Dispose();

    private enum Race { None, Change, Delete }

    private sealed class RacingRepository(AdminRepository inner, MongoDbContext mongo, Race race) : IAdminRepository
    {
        private bool _triggered;

        public async Task<Admin?> GetByIdAsync(Guid id, CancellationToken token)
        {
            var loaded = await inner.GetByIdAsync(id, token);
            if (loaded is not null && !_triggered)
            {
                _triggered = true;
                if (race == Race.Delete)
                    await mongo.Admins.DeleteOneAsync(x => x.Id == id, token);
                else
                {
                    var winner = Admin.Rehydrate(loaded.Id, loaded.ExternalId, "Winner Admin",
                        "winner@example.com", loaded.IsActive, loaded.LastLoginAt, loaded.Version);
                    await inner.UpdateAdmin(winner, token);
                }
            }

            return loaded;
        }

        public Task<Admin?> GetByExternalIdAsync(string externalId, CancellationToken token)
            => inner.GetByExternalIdAsync(externalId, token);
        public Task<Admin> CreateAdmin(Admin admin, CancellationToken token)
            => inner.CreateAdmin(admin, token);
        public Task<Admin> UpdateAdmin(Admin admin, CancellationToken token)
            => inner.UpdateAdmin(admin, token);
    }
}

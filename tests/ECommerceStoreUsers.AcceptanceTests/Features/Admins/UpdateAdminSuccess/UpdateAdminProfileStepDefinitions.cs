using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using Reqnroll;
using Shouldly;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins.UpdateAdminSuccess;

[Binding]
public sealed class UpdateAdminProfileStepDefinitions(ScenarioApiContext context, AdminSuccessState state)
{
    private Guid _adminId;
    private UpdateAdminProfileRequestDto? _request;
    private bool _previouslyApplied;

    [Given("an admin profile exists for update admin request")]
    public async Task GivenTheAdmin(Table table) => _adminId = (await state.SeedAsync(table)).Id;

    [Given("I have a valid update admin request")]
    public void GivenTheRequest(Table table)
    {
        _request = AdminSuccessState.UpdateRequest(table);
        state.Attach("Admin profile update request", _request);
    }

    [Given("this valid admin update has already been applied")]
    public async Task GivenTheUpdateWasAlreadyApplied()
    {
        _request.ShouldNotBeNull();
        await state.ApplyPriorUpdateAsync(_adminId, _request);
        _previouslyApplied = true;
    }

    [When("I submit the valid update admin request")]
    public async Task WhenUpdating()
    {
        _adminId.ShouldNotBe(Guid.Empty);
        _request.ShouldNotBeNull();
        await state.BeginAsync(_previouslyApplied ? "NoOp" : "Update");
        context.Response = await context.HttpClient.PutAsJsonAsync($"/admins/{_adminId}", _request, context.JsonOptions);
        await state.CompleteAsync();
    }

    [Then("the admin profile is updated successfully")]
    public Task ThenTheProfileMatches(Table table) => state.ExpectResponseAsync(table);
}

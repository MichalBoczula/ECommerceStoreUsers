using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Admins;
using Reqnroll;
using Shouldly;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins.CreateAdminSuccess;

[Binding]
public sealed class CreateAdminProfileStepDefinitions(ScenarioApiContext context, AdminSuccessState state)
{
    private CreateAdminRequestDto? _request;

    [Given("I have a valid create admin request")]
    public void GivenTheRequest(Table table)
    {
        _request = AdminSuccessState.CreateRequest(table);
        state.Attach("Admin create request", _request);
    }

    [When("I submit the create admin request")]
    public async Task WhenCreating()
    {
        _request.ShouldNotBeNull();
        await state.BeginAsync("Create");
        context.Response = await context.HttpClient.PostAsJsonAsync("/admins", _request, context.JsonOptions);
        await state.CompleteAsync();
    }

    [Then("the admin profile is created successfully")]
    public Task ThenTheProfileMatches(Table table) => state.ExpectResponseAsync(table);
}

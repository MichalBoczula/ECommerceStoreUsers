using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using Reqnroll;
using Shouldly;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins.GetAdminSuccess;

[Binding]
public sealed class GetAdminProfileStepDefinitions(ScenarioApiContext context, AdminSuccessState state)
{
    private string? _externalId;

    [Given("an admin profile exists for get admin request")]
    public async Task GivenTheAdmin(Table table) => _externalId = (await state.SeedAsync(table)).ExternalId;

    [When("I request the admin profile by external id")]
    public async Task WhenReading(Table table)
    {
        var externalId = AdminSuccessState.Required(AdminSuccessState.Values(table), "ExternalId");
        externalId.ShouldBe(_externalId);
        await state.BeginAsync("Get");
        context.Response = await context.HttpClient.GetAsync($"/admins/external/{Uri.EscapeDataString(externalId)}");
        await state.CompleteAsync();
    }

    [Then("the admin profile is returned successfully")]
    public Task ThenTheProfileMatches(Table table) => state.ExpectResponseAsync(table);
}

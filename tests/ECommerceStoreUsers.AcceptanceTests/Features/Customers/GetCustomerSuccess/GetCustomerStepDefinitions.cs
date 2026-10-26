using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using Reqnroll;
using Shouldly;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.GetCustomerSuccess;

[Binding]
public sealed class GetCustomerStepDefinitions(ScenarioApiContext context, CustomerSuccessState state)
{
    private string? _externalId;

    [Given("a customer profile exists for get customer request")]
    public async Task GivenTheCustomer(Table table) => _externalId = (await state.SeedAsync(table)).ExternalId;

    [When("I request the customer profile by external id")]
    public async Task WhenReading(Table table)
    {
        var externalId = CustomerSuccessState.Required(CustomerSuccessState.Values(table), "ExternalId");
        externalId.ShouldBe(_externalId);
        await state.BeginAsync("Get");
        context.Response = await context.HttpClient.GetAsync($"/customers/external/{Uri.EscapeDataString(externalId)}");
        state.RequestCompleted();
    }

    [Then("the customer profile is returned successfully")]
    public Task ThenTheCustomerMatches(Table table) => state.ExpectResponseAsync(table);
}

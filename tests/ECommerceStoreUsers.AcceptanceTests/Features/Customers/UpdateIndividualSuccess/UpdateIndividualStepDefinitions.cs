using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using Reqnroll;
using Shouldly;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.UpdateIndividualSuccess;

[Binding]
public sealed class UpdateIndividualStepDefinitions(ScenarioApiContext context, CustomerSuccessState state)
{
    private Guid _customerId;
    private UpdateIndividualDataRequestDto? _request;

    [Given("a customer exists for update individual request")]
    public async Task GivenTheCustomer(Table table) => _customerId = (await state.SeedAsync(table)).Id;

    [Given("I have a valid update individual request")]
    public void GivenTheRequest(Table table)
    {
        _request = new UpdateIndividualDataRequestDto { Individual = CustomerSuccessState.Individual(CustomerSuccessState.Values(table)) };
        state.Attach("Customer individual update request", _request);
    }

    [When("I submit the update individual request")]
    public async Task WhenUpdating()
    {
        _customerId.ShouldNotBe(Guid.Empty);
        _request.ShouldNotBeNull();
        await state.BeginAsync("UpdateIndividual");
        context.Response = await context.HttpClient.PutAsJsonAsync($"/customers/{_customerId}/individual", _request, context.JsonOptions);
        state.RequestCompleted();
    }

    [Then("the individual data is updated successfully")]
    public Task ThenTheCustomerMatches(Table table) => state.ExpectResponseAsync(table);
}

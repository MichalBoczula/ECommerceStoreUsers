using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using Reqnroll;
using Shouldly;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.CreateCustomerSuccess;

[Binding]
public sealed class CreateCustomerStepDefinitions(ScenarioApiContext context, CustomerSuccessState state)
{
    private CreateCustomerRequestDto? _request;

    [Given("I have a valid create customer request")]
    public void GivenTheRequest(Table table)
    {
        _request = CustomerSuccessState.Customer(CustomerSuccessState.Values(table));
        state.Attach("Customer create request", _request);
    }

    [When("I submit the create customer request")]
    public async Task WhenCreating()
    {
        _request.ShouldNotBeNull();
        await state.BeginAsync("Create");
        context.Response = await context.HttpClient.PostAsJsonAsync("/customers", _request, context.JsonOptions);
        state.RequestCompleted();
    }

    [Then("the customer profile is created successfully")]
    public Task ThenTheCustomerMatches(Table table) => state.ExpectResponseAsync(table);
}

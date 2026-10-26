using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using Reqnroll;
using Shouldly;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.AddCompanySuccess;

[Binding]
public sealed class AddCompanyStepDefinitions(ScenarioApiContext context, CustomerSuccessState state)
{
    private Guid _customerId;
    private AddCompanyRequestDto? _request;

    [Given("a customer exists for add company request")]
    public async Task GivenTheCustomer(Table table) => _customerId = (await state.SeedAsync(table)).Id;

    [Given("I have a valid add company request")]
    public void GivenTheRequest(Table table)
    {
        _request = CustomerSuccessState.Company(CustomerSuccessState.Values(table));
        state.Attach("Customer company add request", _request);
    }

    [When("I submit the add company request")]
    public async Task WhenAdding()
    {
        _customerId.ShouldNotBe(Guid.Empty);
        _request.ShouldNotBeNull();
        await state.BeginAsync("AddCompany");
        context.Response = await context.HttpClient.PostAsJsonAsync($"/customers/{_customerId}/companies", _request, context.JsonOptions);
        state.RequestCompleted();
    }

    [Then("the company is added to the customer successfully")]
    public Task ThenTheCustomerMatches(Table table) => state.ExpectResponseAsync(table);
}

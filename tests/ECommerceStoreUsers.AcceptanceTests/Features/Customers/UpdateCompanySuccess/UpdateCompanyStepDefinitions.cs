using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
using Reqnroll;
using Shouldly;
using System.Net.Http.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers.UpdateCompanySuccess;

[Binding]
public sealed class UpdateCompanyStepDefinitions(ScenarioApiContext context, CustomerSuccessState state)
{
    private Guid _customerId;
    private Guid _companyId;
    private UpdateCompanyRequestDto? _request;

    [Given("a customer with company exists for update company request")]
    public async Task GivenTheCustomer(Table table)
    {
        var values = CustomerSuccessState.Values(table);
        var customer = await state.SeedAsync(table);
        _customerId = customer.Id;
        _companyId = customer.Companies.Single(company => company.TaxId == CustomerSuccessState.Required(values, "Company.TaxId")).Id;
    }

    [Given("I have a valid update company request")]
    public void GivenTheRequest(Table table)
    {
        var company = CustomerSuccessState.Company(CustomerSuccessState.Values(table));
        _request = new UpdateCompanyRequestDto
        {
            TaxId = company.TaxId,
            CompanyName = company.CompanyName,
            BillingAddress = company.BillingAddress,
            ShippingAddress = company.ShippingAddress
        };
        state.Attach("Customer company update request", _request);
    }

    [When("I submit the update company request")]
    public async Task WhenUpdating()
    {
        _customerId.ShouldNotBe(Guid.Empty);
        _companyId.ShouldNotBe(Guid.Empty);
        _request.ShouldNotBeNull();
        state.TargetCompanyId = _companyId;
        await state.BeginAsync("UpdateCompany");
        context.Response = await context.HttpClient.PutAsJsonAsync($"/customers/{_customerId}/companies/{_companyId}", _request, context.JsonOptions);
        state.RequestCompleted();
    }

    [Then("the company is updated successfully")]
    public Task ThenTheCustomerMatches(Table table) => state.ExpectResponseAsync(table);
}

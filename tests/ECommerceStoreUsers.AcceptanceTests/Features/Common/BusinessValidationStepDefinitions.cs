using Reqnroll;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class BusinessValidationStepDefinitions(BusinessValidationState state)
{
    [Given("representative Users data exists for business validation checks")]
    public Task Seed() => state.SeedAsync();

    [Given("the valid {string} business payload is")]
    public void Template(string family, string json) => state.Template(family, json);

    [Given("the business request uses {string} with method {string} and path {string}")]
    public void Target(string operationId, string method, string path) => state.Target(operationId, method, path);

    [Given("I change only these JSON values")]
    public void Change(Table table) => state.Change(table);

    [Given("another distinct company exists for the business validation request")]
    public Task SecondCompany() => state.AddSecondCompanyAsync();

    [Given("I record all Users data before this business request")]
    public Task Begin() => state.BeginAsync();

    [When("I submit the business validation request")]
    public Task Send() => state.SendAsync();

    [Then("the complete business validation problem contains exactly these errors")]
    public Task Validation(Table table) => state.ValidationAsync(table);

    [Then("the complete business conflict detail is {string}")]
    public Task Conflict(string detail) => state.ConflictAsync(detail);

    [Then("every Users collection is unchanged after business rejection")]
    public Task Unchanged() => state.UnchangedAsync();
}

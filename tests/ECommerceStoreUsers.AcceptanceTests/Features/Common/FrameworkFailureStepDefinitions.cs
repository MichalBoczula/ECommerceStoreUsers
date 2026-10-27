using Reqnroll;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

[Binding]
public sealed class FrameworkFailureStepDefinitions(FrameworkFailureState state)
{
    [Given("representative Users data exists for framework rejection checks")]
    public Task Seed() => state.SeedAsync();

    [Given("I record all Users data before this framework request")]
    public Task Begin() => state.BeginAsync();

    [Given("the framework request is")]
    public void Prepare(Table table) => state.Prepare(table);

    [Given("the exact framework body is")]
    public void Body(string body) => state.SetBody(body);

    [When("I send the framework request")]
    public Task Send() => state.SendAsync();

    [Then("the complete framework problem has these values")]
    public Task Problem(Table table) => state.ExpectProblemAsync(table);

    [Then("every Users collection is unchanged")]
    public Task Unchanged() => state.ExpectUnchangedAsync();
}

using Reqnroll;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Customers;

[Binding]
public sealed class CustomerPersistenceStepDefinitions(CustomerSuccessState state)
{
    [Then("the customer database contains the expected current and history snapshots")]
    public Task ThenTheCustomerDatabaseMatches(Table table) => state.ExpectDatabaseAsync(table);
}

using Reqnroll;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Admins;

[Binding]
public sealed class AdminPersistenceStepDefinitions(AdminSuccessState state)
{
    [Then("the complete unchanged admin profile is returned")]
    public Task ThenTheNoOpProfileMatches(Table table) => state.ExpectResponseAsync(table);

    [Then("the admin database contains the expected current and history snapshots")]
    public Task ThenTheDatabaseMatches(Table table) => state.ExpectDatabaseAsync(table);
}

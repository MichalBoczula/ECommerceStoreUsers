using Reqnroll;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Favorites;

[Binding]
public sealed class FavoritePersistenceStepDefinitions(FavoriteSuccessState state)
{
    [Given("I observe the complete favorite collection")]
    public void Observe() => state.Observe();

    [Given("these dated favorite entries already exist")]
    public Task SeedDated(Table table) => state.SeedDatedAsync(table);

    [Then("the favorite response has these values")]
    public Task Response(Table table) => state.ExpectAddedAsync(table);

    [Then("the favorite list contains exactly these entries in descending date order")]
    public Task List(Table table) => state.ExpectListAsync(table);

    [Then("the favorite list is empty with status 200")]
    public Task EmptyList() => state.ExpectEmptyListAsync();

    [Then("the favorite response is bodyless with status 204")]
    public Task NoContent() => state.ExpectNoContentAsync();

    [Then("the complete favorite collection contains exactly")]
    public void Collection(Table table) => state.ExpectCollection(table);

    [Then("the complete favorite collection is empty")]
    public void EmptyCollection() => state.ExpectEmptyCollection();

    [Then("favorite persistence has this effect")]
    public void Effect(Table table) => state.ExpectEffect(table);
}

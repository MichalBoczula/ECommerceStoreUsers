@allure.description:Covers_every_declared_success_validation_conflict_and_not_found_path_for_favorite_endpoints.
Feature: Favorites endpoints
  Favorite API responses use stable success and custom problem details contracts.

  @allure.label.scenarioId:REF07_AddProductToFavorites_01
  Scenario: Add a product to favorites
    A client adds a product reference to its favorites collection.
    The response returns the created favorite and its client/product identifiers.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 11111111-1111-1111-1111-111111111111 |
      | ProductId | aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa |
    When I add the product to favorites
    Then the added favorite is returned with status 200

  @allure.label.scenarioId:REF07_AddProductToFavorites_02
  Scenario: Add a favorite with invalid identifiers
    A favorite-add request uses empty client and product identifiers.
    The API rejects the identifiers with a bad-request problem.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 00000000-0000-0000-0000-000000000000 |
      | ProductId | 00000000-0000-0000-0000-000000000000 |
    When I add the product to favorites
    Then favorite validation fails with status 400
      | Name                            | Message                               |
      | FavoriteClientIdValidationRule  | ClientId cannot be an empty GUID.     |
      | FavoriteProductIdValidationRule | ProductId cannot be an empty GUID.    |

  @allure.label.scenarioId:REF07_AddProductToFavorites_03
  Scenario: Add a favorite without the required product identifier
    A favorite-add body omits the required product-identifier member.
    Request deserialization returns a safe bad-request problem.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 11111111-1111-1111-1111-111111111111 |
      | ProductId | aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa |
    When I add a favorite without the required product identifier
    Then the malformed favorite payload fails with status 400

  @allure.label.scenarioId:REF07_AddProductToFavorites_04
  Scenario: Add the same product twice
    A client already has the product in its favorites collection.
    Adding the same client/product pair again returns a conflict problem.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 22222222-2222-2222-2222-222222222222 |
      | ProductId | bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb |
    When I add the same product to favorites twice
    Then adding the duplicate favorite fails with status 409

  @allure.label.scenarioId:REF08_AddProductToFavorites_race
  Scenario: Add a favorite inserted by another request after the existence check
    Another request inserts the client/product pair after the initial existence check.
    The losing add returns a conflict and exactly one matching favorite remains.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 22222222-2222-2222-2222-222222222222 |
      | ProductId | bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb |
    And another request inserts the favorite after the existence check
    When I add the product to favorites
    Then adding the duplicate favorite fails with status 409
    And exactly one matching favorite remains

  @allure.label.scenarioId:REF07_GetFavoritesByClientId_01
  Scenario: Get a client's favorites
    A client has favorite product references stored in its own collection view.
    The list request returns the client's favorites.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 33333333-3333-3333-3333-333333333333 |
      | ProductId | cccccccc-cccc-cccc-cccc-cccccccccccc |
    And the product is already in favorites
    When I get the client's favorites
    Then the favorite list is returned with status 200

  @allure.label.scenarioId:REF07_GetFavoritesByClientId_02
  Scenario: Get favorites with an empty client identifier
    A favorites lookup uses an empty client identifier.
    The API rejects the identifier with a bad-request problem.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 00000000-0000-0000-0000-000000000000 |
      | ProductId | cccccccc-cccc-cccc-cccc-cccccccccccc |
    When I get the client's favorites
    Then favorite validation fails with status 400
      | Name          | Message               |
      | EmptyGuidRule | Guid cannot be empty. |

  @allure.label.scenarioId:REF07_RemoveProductFromFavorites_01
  Scenario: Remove a product from favorites
    A client removes an existing product reference from its favorites.
    The delete succeeds with a bodyless response and the favorite no longer appears in the list.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 44444444-4444-4444-4444-444444444444 |
      | ProductId | dddddddd-dddd-dddd-dddd-dddddddddddd |
    And the product is already in favorites
    When I remove the product from favorites
    Then the favorite is removed with status 204

  @allure.label.scenarioId:REF07_RemoveProductFromFavorites_02
  Scenario: Remove a favorite with empty identifiers
    A favorite-removal request uses empty client and product identifiers.
    The API rejects the identifiers with a bad-request problem.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 00000000-0000-0000-0000-000000000000 |
      | ProductId | 00000000-0000-0000-0000-000000000000 |
    When I remove the product from favorites
    Then favorite validation fails with status 400
      | Name          | Message               |
      | EmptyGuidRule | Guid cannot be empty. |
      | EmptyGuidRule | Guid cannot be empty. |

  @allure.label.scenarioId:REF07_RemoveProductFromFavorites_03
  Scenario: Remove a favorite that does not exist
    The requested client/product favorite pair is absent.
    The delete returns a not-found problem.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 55555555-5555-5555-5555-555555555555 |
      | ProductId | eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee |
    When I remove the product from favorites
    Then the missing favorite fails with status 404

  @allure.label.scenarioId:REF08_RemoveProductFromFavorites_race
  Scenario: Remove a favorite deleted by another request after loading it
    Another request removes the favorite after the delete operation loads it.
    The losing delete returns not found and no matching favorite remains.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 55555555-5555-5555-5555-555555555555 |
      | ProductId | eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee |
    And the product is already in favorites
    And another request deletes the favorite after it is loaded
    When I remove the product from favorites
    Then the missing favorite fails with status 404
    And no matching favorite remains

  @allure.label.scenarioId:REF07_ClearClientFavorites_01
  Scenario: Clear all favorites for a client
    A client clears its favorite product references.
    The API returns a bodyless success and the client's list becomes empty.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 66666666-6666-6666-6666-666666666666 |
      | ProductId | ffffffff-ffff-ffff-ffff-ffffffffffff |
    And the client has two favorite products
      | ProductId                            |
      | ffffffff-ffff-ffff-ffff-ffffffffffff |
      | 77777777-7777-7777-7777-777777777777 |
    When I clear the client's favorites
    Then all favorites are removed with status 204

  @allure.label.scenarioId:REF07_ClearClientFavorites_noop
  Scenario: Clear favorites for a client with no products is a successful no-op
    A client has no favorites to remove.
    Clearing the empty collection still succeeds with a bodyless response.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 88888888-8888-8888-8888-888888888888 |
      | ProductId | 99999999-9999-9999-9999-999999999999 |
    When I clear the client's favorites
    Then all favorites are removed with status 204

  @allure.label.scenarioId:REF07_ClearClientFavorites_02
  Scenario: Clear favorites with an empty client identifier
    A favorites-clear request uses an empty client identifier.
    The API rejects the identifier with a bad-request problem.
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 00000000-0000-0000-0000-000000000000 |
      | ProductId | ffffffff-ffff-ffff-ffff-ffffffffffff |
    When I clear the client's favorites
    Then favorite validation fails with status 400
      | Name          | Message               |
      | EmptyGuidRule | Guid cannot be empty. |

@allure.description:Covers_every_declared_success_validation_conflict_and_not_found_path_for_favorite_endpoints.
Feature: Favorites endpoints
  Favorite API responses use stable success and custom problem details contracts.

  Scenario: Add a product to favorites
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 11111111-1111-1111-1111-111111111111 |
      | ProductId | aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa |
    When I add the product to favorites
    Then the added favorite is returned with status 200

  Scenario: Add a favorite with invalid identifiers
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 00000000-0000-0000-0000-000000000000 |
      | ProductId | 00000000-0000-0000-0000-000000000000 |
    When I add the product to favorites
    Then favorite validation fails with status 400
      | Name                            | Message                               |
      | FavoriteClientIdValidationRule  | ClientId cannot be an empty GUID.     |
      | FavoriteProductIdValidationRule | ProductId cannot be an empty GUID.    |

  Scenario: Add a favorite without the required product identifier
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 11111111-1111-1111-1111-111111111111 |
      | ProductId | aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa |
    When I add a favorite without the required product identifier
    Then the malformed favorite payload fails with status 400

  Scenario: Add the same product twice
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 22222222-2222-2222-2222-222222222222 |
      | ProductId | bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb |
    When I add the same product to favorites twice
    Then adding the duplicate favorite fails with status 409

  Scenario: Get a client's favorites
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 33333333-3333-3333-3333-333333333333 |
      | ProductId | cccccccc-cccc-cccc-cccc-cccccccccccc |
    And the product is already in favorites
    When I get the client's favorites
    Then the favorite list is returned with status 200

  Scenario: Get favorites with an empty client identifier
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 00000000-0000-0000-0000-000000000000 |
      | ProductId | cccccccc-cccc-cccc-cccc-cccccccccccc |
    When I get the client's favorites
    Then favorite validation fails with status 400
      | Name          | Message               |
      | EmptyGuidRule | Guid cannot be empty. |

  Scenario: Remove a product from favorites
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 44444444-4444-4444-4444-444444444444 |
      | ProductId | dddddddd-dddd-dddd-dddd-dddddddddddd |
    And the product is already in favorites
    When I remove the product from favorites
    Then the favorite is removed with status 204

  Scenario: Remove a favorite with empty identifiers
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 00000000-0000-0000-0000-000000000000 |
      | ProductId | 00000000-0000-0000-0000-000000000000 |
    When I remove the product from favorites
    Then favorite validation fails with status 400
      | Name          | Message               |
      | EmptyGuidRule | Guid cannot be empty. |
      | EmptyGuidRule | Guid cannot be empty. |

  Scenario: Remove a favorite that does not exist
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 55555555-5555-5555-5555-555555555555 |
      | ProductId | eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee |
    When I remove the product from favorites
    Then the missing favorite fails with status 404

  Scenario: Clear all favorites for a client
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

  Scenario: Clear favorites with an empty client identifier
    Given I use favorite identifiers
      | Field     | Value                                |
      | ClientId  | 00000000-0000-0000-0000-000000000000 |
      | ProductId | ffffffff-ffff-ffff-ffff-ffffffffffff |
    When I clear the client's favorites
    Then favorite validation fails with status 400
      | Name          | Message               |
      | EmptyGuidRule | Guid cannot be empty. |

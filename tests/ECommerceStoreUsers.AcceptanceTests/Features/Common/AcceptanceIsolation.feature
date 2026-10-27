Feature: Users acceptance scenario isolation
  Each scenario uses its own MongoDB database for current and history documents.

  @allure.label.scenarioId:REF05_Isolation_CurrentHistory
  Scenario Outline: A previous scenario cannot leave customer documents in this scenario (<run>)
    Each example starts with an isolated scenario database and no markers from a previous example.
    Direct MongoDB writes leave exactly this example's fully identified marker in each of all five Users collections, including current and history; no business HTTP request is made.
    Given this Users scenario has no REF-05 customer marker in current or history
    When I store a REF-05 customer marker in current and history
    Then this Users scenario contains only its own REF-05 markers

    Examples:
      | run    |
      | first  |
      | second |

  @allure.label.scenarioId:REF05_Isolation_DependencyFailure
  Scenario: A handled dependency failure does not write customer documents
    A customer lookup runs against a deliberately failing dependency in an isolated database.
    The safe server-error response leaves the current and history marker counts unchanged.
    Given this Users scenario has no REF-05 customer marker in current or history
    And versioned Users data exists before a technical operation
    And the "customer" dependency fails unexpectedly
    When I request the failing "customer" endpoint
    Then a safe custom server error is returned
      | Field      | Value                         |
      | StatusCode | 500                           |
      | Title      | Server error.                 |
      | Detail     | An unexpected error occurred. |
    And this Users scenario has no REF-05 customer marker in current or history
    And the complete technical server problem is safe
    And the technical operation leaves all Users collections unchanged


  @allure.label.scenarioId:REF09_Isolation_TwoHosts
  Scenario: Two hosts isolate the same external identifier and peer cleanup preserves the main host
    Two real application hosts share the replica set but use different scenario databases.
    Each creates its own complete Admin and Insert history with the same ExternalId, demonstrating that unique indexes and business data are scoped to the database.
    Removing the peer host and peer database leaves the main host's full profile and all collections unchanged; this explicitly verifies peer cleanup rather than inferring cross-scenario execution order.
    Given a peer Users host uses another scenario database
    When both hosts create the same external identifier with these profiles
      | Host | ExternalId      | FullName   | Email            |
      | Main | shared-isolated | Main Admin | main@example.com |
      | Peer | shared-isolated | Peer Admin | peer@example.com |
    Then each host has exactly its own complete Admin and Insert history
    When the peer host is stopped and its database is removed
    Then the main host still returns its full profile with unchanged stored data

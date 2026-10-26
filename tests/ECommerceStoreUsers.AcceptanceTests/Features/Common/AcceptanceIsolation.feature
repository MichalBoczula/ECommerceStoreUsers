Feature: Users acceptance scenario isolation
  Each scenario uses its own MongoDB database for current and history documents.

  @allure.label.scenarioId:REF05_Isolation_CurrentHistory
  Scenario Outline: A previous scenario cannot leave customer documents in this scenario (<run>)
    Each example starts with an isolated scenario database and no markers from a previous example.
    Direct MongoDB writes leave only this example's marker in current and history collections; no business HTTP request is made.
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
    And the "customer" dependency fails unexpectedly
    When I request the failing "customer" endpoint
    Then a safe custom server error is returned
      | Field      | Value                         |
      | StatusCode | 500                           |
      | Title      | Server error.                 |
      | Detail     | An unexpected error occurred. |
    And this Users scenario has no REF-05 customer marker in current or history

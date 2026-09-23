Feature: Users acceptance scenario isolation
  Each scenario uses its own MongoDB database for current and history documents.

  Scenario Outline: A previous scenario cannot leave customer documents in this scenario (<run>)
    Given this Users scenario has no REF-05 customer marker in current or history
    When I store a REF-05 customer marker in current and history
    Then this Users scenario contains only its own REF-05 markers

    Examples:
      | run    |
      | first  |
      | second |

  Scenario: A handled dependency failure does not write customer documents
    Given this Users scenario has no REF-05 customer marker in current or history
    And the "customer" dependency fails unexpectedly
    When I request the failing "customer" endpoint
    Then a safe custom server error is returned
      | Field      | Value                         |
      | StatusCode | 500                           |
      | Title      | Server error.                 |
      | Detail     | An unexpected error occurred. |
    And this Users scenario has no REF-05 customer marker in current or history

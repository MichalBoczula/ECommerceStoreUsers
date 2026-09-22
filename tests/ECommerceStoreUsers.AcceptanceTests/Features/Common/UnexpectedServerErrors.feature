@allure.description:Ensures_unexpected_dependency_failures_return_a_safe_custom_problem_details_response_without_exposing_internal_information.
Feature: Safe unexpected server errors
  Unexpected dependency failures return a stable custom problem details contract.
  Exception types, messages and stack traces must not be exposed to API consumers.

  Scenario Outline: Endpoint returns a safe 500 response when its dependency fails
    Given the "<area>" dependency fails unexpectedly
    When I request the failing "<area>" endpoint
    Then a safe custom server error is returned
      | Field      | Value                         |
      | StatusCode | 500                           |
      | Title      | Server error.                 |
      | Detail     | An unexpected error occurred. |

    Examples:
      | area                      |
      | customer                  |
      | admin                     |
      | favorites                 |
      | favorite-add              |
      | favorite-remove           |
      | favorite-clear            |
      | documentation-flows       |
      | documentation-validations |

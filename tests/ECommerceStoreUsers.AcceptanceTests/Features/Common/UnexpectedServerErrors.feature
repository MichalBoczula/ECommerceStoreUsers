@allure.description:Ensures_unexpected_dependency_failures_return_a_safe_custom_problem_details_response_without_exposing_internal_information.
Feature: Safe unexpected server errors
  Unexpected dependency failures return a stable custom problem details contract.
  Exception types, messages and stack traces must not be exposed to API consumers.

  Scenario Outline: Endpoint returns a safe 500 response when its dependency fails
    An endpoint is invoked with a deliberately failing repository or documentation provider.
    The response reports a safe server error without exposing the injected exception's internal details.
    Given versioned Users data exists before a technical operation
    And the "<area>" dependency fails unexpectedly
    When I request the failing "<area>" endpoint
    Then a safe custom server error is returned
      | Field      | Value                         |
      | StatusCode | 500                           |
      | Title      | Server error.                 |
      | Detail     | An unexpected error occurred. |

    And the complete technical server problem is safe
    And the technical operation leaves all Users collections unchanged

    Examples:
      | area                      | scenarioId                          |
      | customer                  | REF07_GetCustomerByExternalId_04    |
      | admin                     | REF07_GetAdminByExternalId_04       |
      | favorites                 | REF07_GetFavoritesByClientId_03     |
      | favorite-add              | REF07_AddProductToFavorites_05      |
      | favorite-remove           | REF07_RemoveProductFromFavorites_04 |
      | favorite-clear            | REF07_ClearClientFavorites_03       |
      | documentation-flows       | REF07_GetFlowDocumentation_02       |
      | documentation-validations | REF07_GetValidationDocumentation_02 |

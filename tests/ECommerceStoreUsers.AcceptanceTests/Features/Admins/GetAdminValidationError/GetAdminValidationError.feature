@allure.description:Ensures_getting_an_admin_profile_with_invalid_external_id_returns_a_bad_request_with_validation_errors.
Feature: Get admin profile validation

  @allure.label.scenarioId:REF07_GetAdminByExternalId_03
  Scenario: Get admin profile with blank external id returns bad request
    An administrator lookup supplies a blank external-identity value.
    The API returns a bad-request validation problem.
    Given I have an invalid get admin external id
      | Field      | Value       |
      | ExternalId | <whitespace> |
    And I record all Users data before this business request
    When I submit the get admin request for validation
    Then the get admin request fails with a validation error
      | Field      | Value                                                    |
      | StatusCode | 400                                                      |
      | Message    | ExternalId cannot be null or white space.                |
      | Name       | AdminExternalIdValidationRule                           |
      | Entity     | Admin                                                    |
    And the complete business validation problem contains exactly these errors
      | Name                          | Message                                   | Entity |
      | AdminExternalIdValidationRule | ExternalId cannot be null or white space. | Admin  |
    And every Users collection is unchanged after business rejection

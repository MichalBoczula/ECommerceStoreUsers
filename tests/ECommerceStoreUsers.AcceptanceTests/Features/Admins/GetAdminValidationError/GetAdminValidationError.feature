@allure.description:Ensures_getting_an_admin_profile_with_invalid_external_id_returns_a_bad_request_with_validation_errors.
Feature: Get admin profile validation

  Scenario: Get admin profile with blank external id returns bad request
    Given I have an invalid get admin external id
      | Field      | Value       |
      | ExternalId | <whitespace> |
    When I submit the get admin request for validation
    Then the get admin request fails with a validation error
      | Field      | Value                                                    |
      | StatusCode | 400                                                      |
      | Message    | ExternalId cannot be null or white space.                |
      | Name       | AdminExternalIdValidationRule                           |
      | Entity     | Admin                                                    |

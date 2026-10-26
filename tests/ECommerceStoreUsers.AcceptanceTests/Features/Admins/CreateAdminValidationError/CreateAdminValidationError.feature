@allure.description:Ensures_creating_an_admin_profile_with_invalid_email_returns_a_bad_request_with_validation_errors.
Feature: Create admin profile validation

  @allure.label.scenarioId:REF07_CreateAdmin_04
  Scenario: Create admin profile with invalid email returns bad request
    An administrator registration supplies an invalid email address.
    The API returns the email-validation error in a bad-request problem.
    Given I have an invalid create admin request
      | Field      | Value          |
      | ExternalId | auth-678901234  |
      | FullName   | Jane Smith     |
      | Email      | invalid-email  |
    When I submit the create admin request for validation
    Then the admin profile creation fails with a validation error
      | Field      | Value                                                       |
      | StatusCode | 400                                                         |
      | Message    | Email must be a valid format (address@domain.something).    |
      | Name       | AdminEmailValidationRule                                    |
      | Entity     | Admin                                                       |
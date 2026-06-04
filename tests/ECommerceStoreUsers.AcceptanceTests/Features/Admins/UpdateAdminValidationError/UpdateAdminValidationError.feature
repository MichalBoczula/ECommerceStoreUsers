@allure.description:Ensures_updating_an_admin_profile_with_invalid_email_returns_a_bad_request_with_validation_errors.
Feature: Update admin profile validation

  Scenario: Update admin profile with invalid email returns bad request
    Given I have an existing admin profile and an invalid update admin request
      | Field                | Value                  |
      | ExistingExternalId   | auth-update-678901234  |
      | ExistingFullName     | Jane Smith             |
      | ExistingEmail        | jane.smith@db.com      |
      | UpdatedFullName      | Jane Smith Updated     |
      | UpdatedEmail         | invalid-email          |
    When I submit the update admin request for validation
    Then the admin profile update fails with a validation error
      | Field      | Value                                                       |
      | StatusCode | 400                                                         |
      | Message    | Email must be a valid format (address@domain.something).    |
      | Name       | AdminEmailValidationRule                                    |
      | Entity     | Admin                                                       |

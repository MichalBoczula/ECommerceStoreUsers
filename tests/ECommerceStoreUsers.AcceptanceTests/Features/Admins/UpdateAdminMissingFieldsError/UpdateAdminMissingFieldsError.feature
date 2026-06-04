@allure.description:Ensures_updating_an_admin_profile_with_a_payload_missing_required_json_properties_returns_a_bad_request.
Feature: Update admin profile deserialization validation

  Scenario: Update admin profile with missing full name returns deserialization error
    Given I have an existing admin profile and an update admin request payload missing the full name
      | Field              | Value                         |
      | ExistingExternalId | auth-update-missing-456789    |
      | ExistingFullName   | Existing Missing Field Admin  |
      | ExistingEmail      | existing.missing.admin@db.com |
      | Email              | updated.missing.admin@db.com  |
    When I submit the incomplete update admin request
    Then the update admin response indicates a json deserialization failure
      | Field           | Value                                                                 |
      | StatusCode      | 400                                                                   |
      | Title           | Invalid JSON payload.                                                 |
      | Detail          | JSON payload for UpdateAdminProfileRequestDto is missing required properties: fullName. |
      | Type            | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1           |
      | MissingProperty | fullName                                                              |

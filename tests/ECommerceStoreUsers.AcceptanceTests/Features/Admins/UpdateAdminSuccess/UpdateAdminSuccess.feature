@allure.description:Ensures_updating_an_existing_admin_profile_returns_a_success_response_with_the_updated_payload_fields.
Feature: Update admin profile

  @allure.label.scenarioId:REF07_UpdateAdminProfile_03
  Scenario: Update admin profile returns updated admin data
    An existing administrator changes full name and email address.
    The API returns the profile with the revised values.
    Given an admin profile exists for update admin request
      | Field      | Value                  |
      | ExternalId | auth-update-123456     |
      | FullName   | Original Admin Success |
      | Email      | original.admin@db.com  |
    And I have a valid update admin request
      | Field    | Value                 |
      | FullName | Updated Admin Success |
      | Email    | updated.admin@db.com  |
    When I submit the valid update admin request
    Then the admin profile is updated successfully
      | Field          | Value                 |
      | StatusCode     | 200                   |
      | HasId          | true                  |
      | ExternalId     | auth-update-123456    |
      | FullName       | Updated Admin Success |
      | Email          | updated.admin@db.com  |
      | IsActive       | true                  |
      | HasLastLoginAt | true                  |

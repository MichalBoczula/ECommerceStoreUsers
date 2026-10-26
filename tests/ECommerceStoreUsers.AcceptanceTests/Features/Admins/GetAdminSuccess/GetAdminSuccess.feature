@allure.description:Ensures_getting_an_existing_admin_profile_returns_a_success_response_with_the_admin_payload_fields.
Feature: Get admin profile

  @allure.label.scenarioId:REF07_GetAdminByExternalId_02
  Scenario: Get admin profile by external id returns existing admin data
    An administrator profile already exists for the requested external identity.
    The lookup returns that administrator's profile data.
    Given an admin profile exists for get admin request
      | Field      | Value             |
      | ExternalId | auth-get-123456   |
      | FullName   | Get Admin Success |
      | Email      | get.admin@db.com  |
    When I request the admin profile by external id
      | Field      | Value           |
      | ExternalId | auth-get-123456 |
    Then the admin profile is returned successfully
      | Field          | Value             |
      | StatusCode     | 200               |
      | HasId          | true              |
      | ExternalId     | auth-get-123456   |
      | FullName       | Get Admin Success |
      | Email          | get.admin@db.com  |
      | IsActive       | true              |
      | HasLastLoginAt | true              |

@allure.description:Ensures_getting_an_existing_admin_profile_returns_a_success_response_with_the_admin_payload_fields.
Feature: Get admin profile

  Scenario: Get admin profile by external id returns existing admin data
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

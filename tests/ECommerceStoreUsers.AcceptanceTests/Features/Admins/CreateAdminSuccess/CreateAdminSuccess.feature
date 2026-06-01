@allure.description:Ensures_creating_an_admin_profile_returns_a_success_response_with_the_created_payload_fields.
Feature: Create admin profile

  Scenario: Create admin profile returns created admin data
    Given I have a valid create admin request
      | Field      | Value             |
      | ExternalId | auth-678901234     |
      | FullName   | Jane Smith        |
      | Email      | jane.smith@db.com |
    When I submit the create admin request
    Then the admin profile is created successfully
      | Field      | Value             |
      | StatusCode | 200               |
      | HasId      | true              |
      | ExternalId | auth-678901234     |
      | FullName   | Jane Smith        |
      | Email      | jane.smith@db.com |

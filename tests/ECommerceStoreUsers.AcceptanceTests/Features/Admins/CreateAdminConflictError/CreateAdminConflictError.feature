@allure.description:Ensures_creating_a_duplicate_admin_profile_returns_a_conflict_response.
Feature: Create admin profile conflict

  Scenario: Create duplicate admin profile returns conflict response
    Given I have a valid create admin request payload
      | Field      | Value             |
      | ExternalId | auth-999999999     |
      | FullName   | Duplicate Admin   |
      | Email      | duplicate@db.com  |
    And an admin profile with the same external id already exists
    When I submit the duplicate create admin request
    Then the admin profile creation fails with a conflict error
      | Field      | Value             |
      | StatusCode | 409               |
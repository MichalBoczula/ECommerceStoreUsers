@allure.description:Ensures_creating_a_duplicate_admin_profile_returns_a_conflict_response.
Feature: Create admin profile conflict

  @allure.label.scenarioId:REF07_CreateAdmin_01
  Scenario: Create duplicate admin profile returns conflict response
    An administrator profile already exists for the supplied external identity.
    A second registration returns a conflict problem.
    Given I have a valid create admin request payload
      | Field      | Value             |
      | ExternalId | auth-999999999     |
      | FullName   | Duplicate Admin   |
      | Email      | duplicate@db.com  |
    And an admin profile with the same external id already exists
    And I record all Users data before this business request
    When I submit the duplicate create admin request
    Then the admin profile creation fails with a conflict error
      | Field      | Value             |
      | StatusCode | 409               |
    And the complete business conflict detail is "Resource Admin identified by id auth-999999999 already exists in db. Error in action CreateAdmin."
    And every Users collection is unchanged after business rejection

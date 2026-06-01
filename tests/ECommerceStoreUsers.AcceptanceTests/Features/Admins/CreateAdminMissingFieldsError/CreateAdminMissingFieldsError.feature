@allure.description:Ensures_submitting_a_payload_missing_required_json_properties_returns_a_bad_request.
Feature: Create admin profile deserialization validation

  Scenario: Create admin profile with missing external id returns deserialization error
    Given I have a create admin request payload missing the external id
      | Field    | Value             |
      | FullName | Jane Smith        |
      | Email    | jane.smith@db.com |
    When I submit the incomplete create admin request
    Then the response indicates a json deserialization failure
      | Field      | Value             |
      | StatusCode | 400               |
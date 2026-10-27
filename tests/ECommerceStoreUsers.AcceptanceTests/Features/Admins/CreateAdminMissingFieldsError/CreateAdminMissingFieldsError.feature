@allure.description:Ensures_submitting_a_payload_missing_required_json_properties_returns_a_bad_request.
Feature: Create admin profile deserialization validation

  @allure.label.scenarioId:REF07_CreateAdmin_02
  Scenario: Create admin profile with missing external id returns deserialization error
    An administrator registration omits the required external-identity member.
    Request deserialization rejects the body with a safe bad-request problem.
    Given I have a create admin request payload missing the external id
      | Field    | Value             |
      | FullName | Jane Smith        |
      | Email    | jane.smith@db.com |
    And I record all Users data before this framework request
    When I submit the incomplete create admin request
    Then the response indicates a json deserialization failure
      | Field      | Value             |
      | StatusCode | 400               |
    And the complete framework problem has these values
      | Field             | Value                                                                              |
      | Status            | 400                                                                                |
      | Code              | invalid_json                                                                       |
      | Title             | Invalid JSON payload.                                                              |
      | Detail            | JSON payload for CreateAdminRequestDto is missing required properties: externalId. |
      | MissingProperties | externalId                                                                         |
      | Allow             | []                                                                                 |
    And every Users collection is unchanged

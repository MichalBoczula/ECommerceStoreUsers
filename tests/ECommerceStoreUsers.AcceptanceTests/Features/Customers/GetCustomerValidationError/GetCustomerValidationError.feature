@allure.description:Ensures_getting_a_customer_profile_with_empty_guid_external_id_returns_a_bad_request_with_validation_errors.
Feature: Get customer profile validation

  @allure.label.scenarioId:REF07_GetCustomerByExternalId_03
  Scenario: Get customer profile with empty guid external id returns bad request
    A customer lookup uses the empty-GUID external identity value.
    The API rejects that value with a validation problem.
    Given I have an invalid get customer request
      | Field      | Value                                |
      | ExternalId | 00000000-0000-0000-0000-000000000000 |
    And I record all Users data before this business request
    When I submit the get customer request for validation
    Then the get customer request fails with a validation error
      | Field      | Value                                    |
      | StatusCode | 400                                      |
      | Message    | ExternalId cannot be an empty guid.      |
      | Name       | CustomerExternalIdValidationRule         |
      | Entity     | Customer                                 |
    And the complete business validation problem contains exactly these errors
      | Name                             | Message                             | Entity   |
      | CustomerExternalIdValidationRule | ExternalId cannot be an empty guid. | Customer |
    And every Users collection is unchanged after business rejection

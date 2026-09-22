@allure.description:Ensures_documentation_endpoints_expose_every_registered_flow_and_validation_policy.
Feature: Documentation endpoints
  Runtime documentation must expose complete flow and validation metadata.

  Scenario: Get all flow descriptors
    When I request the flow documentation
    Then all 12 flow descriptors are returned with status 200

  Scenario: Get all validation descriptors
    When I request the validation documentation
    Then all 6 validation policies are returned with status 200

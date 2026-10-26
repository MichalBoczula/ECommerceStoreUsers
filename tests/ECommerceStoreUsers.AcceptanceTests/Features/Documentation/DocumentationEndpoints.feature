@allure.description:Ensures_documentation_endpoints_expose_every_registered_flow_and_validation_policy.
Feature: Documentation endpoints
  Runtime documentation must expose complete flow and validation metadata.

  @allure.label.scenarioId:REF07_GetFlowDocumentation_01
  Scenario: Get all flow descriptors
    A documentation consumer requests the service's published application flows.
    The response includes all twelve registered customer, administrator and favorites descriptors.
    When I request the flow documentation
    Then all 12 flow descriptors are returned with status 200

  @allure.label.scenarioId:REF07_GetValidationDocumentation_01
  Scenario: Get all validation descriptors
    A documentation consumer requests the service's published validation policies.
    The response includes all six registered policy descriptors.
    When I request the validation documentation
    Then all 6 validation policies are returned with status 200

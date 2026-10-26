@allure.description:Ensures_the_service_health_endpoint_is_available.
Feature: Health endpoint
  The service exposes its health status for platform probes.

  @allure.label.scenarioId:REF09_Health_Compatibility
  Scenario: Health endpoint returns success
    A platform probe calls the compatibility health route.
    The live service returns a successful plain-text health response.
    When I request the service health endpoint
    Then the health response status is 200

  @allure.label.scenarioId:REF09_Health_Readiness
  Scenario: Replica set is ready for transactional writes
    A readiness probe runs while the test MongoDB replica set is available.
    The readiness route returns a successful health response.
    When I request the ready health endpoint
    Then the health response status is 200

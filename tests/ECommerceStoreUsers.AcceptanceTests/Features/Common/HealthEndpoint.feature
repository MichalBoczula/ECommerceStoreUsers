@allure.description:Ensures_the_service_health_endpoint_is_available.
Feature: Health endpoint
  The service exposes its health status for platform probes.

  @allure.label.scenarioId:REF09_Health_Compatibility
  Scenario: Health endpoint returns success
    A platform probe calls the compatibility health route.
    The live service returns a successful plain-text health response.
    Given versioned Users data exists before a technical operation
    When I request the service health endpoint
    Then the health response status is 200
    And the health response contains these exact values
      | Field       | Value        |
      | Path        | /health  |
      | Status      | 200          |
      | ContentType | text/plain   |
      | Body        | Healthy      |
    And the technical operation leaves all Users collections unchanged

  @allure.label.scenarioId:REF09_Health_Readiness
  Scenario: Replica set is ready for transactional writes
    A readiness probe runs while the test MongoDB replica set is available.
    The readiness route returns a successful health response.
    Given versioned Users data exists before a technical operation
    When I request the ready health endpoint
    Then the health response status is 200
    And the health response contains these exact values
      | Field       | Value        |
      | Path        | /health/ready  |
      | Status      | 200          |
      | ContentType | text/plain   |
      | Body        | Healthy      |
    And the real MongoDB topology supports transactional writes
    And the technical operation leaves all Users collections unchanged


  @allure.label.scenarioId:REF09_Health_Liveness
  Scenario: Explicit liveness returns the process health text
    The live host exposes the explicit process probe independently of transaction readiness.
    This available-replica-set scenario verifies exact text and caching headers; the standalone readiness facts verify Mongo-unavailable independence.
    Given versioned Users data exists before a technical operation
    When I request health route "/health/live"
    Then the health response contains these exact values
      | Field       | Value        |
      | Path        | /health/live |
      | Status      | 200          |
      | ContentType | text/plain   |
      | Body        | Healthy      |
    And the technical operation leaves all Users collections unchanged

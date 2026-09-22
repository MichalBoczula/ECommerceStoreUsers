@allure.description:Ensures_the_service_health_endpoint_is_available.
Feature: Health endpoint
  The service exposes its health status for platform probes.

  Scenario: Health endpoint returns success
    When I request the service health endpoint
    Then the health response status is 200

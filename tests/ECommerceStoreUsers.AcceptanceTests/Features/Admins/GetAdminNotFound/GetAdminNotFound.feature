@allure.description:Ensures_getting_a_missing_admin_profile_by_external_id_returns_a_not_found_problem_details_response.
Feature: Get admin profile not found

  @allure.label.scenarioId:REF07_GetAdminByExternalId_01
  Scenario: Get admin profile by missing external id returns not found response
    No administrator profile matches the requested external identity.
    The lookup returns a not-found problem.
    Given I have a missing admin external id
      | Field      | Value                  |
      | ExternalId | auth-missing-404-admin |
    When I request the admin profile by external id
    Then the admin profile request fails with a not found response
      | Field      | Value                                                                                                                                                 |
      | StatusCode | 404                                                                                                                                                   |
      | Title      | Resource not found.                                                                                                                                   |
      | Detail     | Resource Admin identified by id auth-missing-404-admin cannot be found in database during action GetAdminByExternalId.                                 |
      | Type       | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4                                                                                           |
      | Instance   | /admins/external/auth-missing-404-admin                                                                                                                        |
      | HasTraceId | true                                                                                                                                                  |

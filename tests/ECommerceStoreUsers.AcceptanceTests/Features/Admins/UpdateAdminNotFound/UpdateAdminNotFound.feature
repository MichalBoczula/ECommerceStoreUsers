@allure.description:Ensures_updating_a_missing_admin_profile_by_admin_id_returns_a_not_found_problem_details_response.
Feature: Update admin profile not found

  @allure.label.scenarioId:REF07_UpdateAdminProfile_02
  Scenario: Update admin profile by missing admin id returns not found response
    An administrator update targets an identifier that does not exist.
    The API returns a not-found problem.
    Given I have a missing admin update request
      | Field     | Value                                |
      | AdminId   | 11111111-1111-1111-1111-111111111111 |
      | FullName  | Missing Update Admin                 |
      | Email     | missing.update.admin@db.com          |
    When I submit the missing admin update request
    Then the update admin profile request fails with a not found response
      | Field      | Value                                                                                                                                                 |
      | StatusCode | 404                                                                                                                                                   |
      | Title      | Resource not found.                                                                                                                                   |
      | Detail     | Resource Admin identified by id 11111111-1111-1111-1111-111111111111 cannot be found in database during action UpdateAdminProfile.                    |
      | Type       | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4                                                                                           |
      | Instance   | /admins/11111111-1111-1111-1111-111111111111                                                                                                         |
      | HasTraceId | true                                                                                                                                                  |

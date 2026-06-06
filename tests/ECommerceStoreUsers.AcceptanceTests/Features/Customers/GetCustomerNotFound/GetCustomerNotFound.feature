@allure.description:Ensures_getting_a_missing_customer_profile_by_external_id_returns_a_not_found_problem_details_response.
Feature: Get customer profile not found

  Scenario: Get customer profile by missing external id returns not found response
    Given I have a missing customer external id
      | Field      | Value                     |
      | ExternalId | auth-missing-404-customer |
    When I request the customer profile by external id
    Then the customer profile request fails with a not found response
      | Field      | Value                                                                                                                                                        |
      | StatusCode | 404                                                                                                                                                          |
      | Title      | Resource not found.                                                                                                                                          |
      | Detail     | Resource Customer identified by id auth-missing-404-customer cannot be found in database during action GetCustomerByExternalId.                               |
      | Type       | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4                                                                                                  |
      | Instance   | /customers/external/auth-missing-404-customer                                                                                                                |
      | HasTraceId | true                                                                                                                                                         |

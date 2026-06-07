@allure.description:Ensures_updating_individual_data_for_a_missing_customer_by_customer_id_returns_a_not_found_problem_details_response.
Feature: Update customer individual data not found

  Scenario: Update individual data by missing customer id returns not found response
    Given I have a missing customer update individual request
      | Field                                    | Value                                |
      | CustomerId                               | 33333333-3333-3333-3333-333333333333 |
      | Individual.FirstName                     | Missing                              |
      | Individual.LastName                      | Individual                           |
      | Individual.Email                         | missing.individual.customer@db.com   |
      | Individual.Phone                         | +48123123123                         |
      | Individual.BillingAddress.PostalCode     | 00-950                               |
      | Individual.BillingAddress.City           | Warsaw                               |
      | Individual.BillingAddress.Street         | Prosta                               |
      | Individual.BillingAddress.BuildingNumber | 20                                   |
      | Individual.BillingAddress.ApartmentNumber | 15                                   |
      | Individual.ShippingAddress.PostalCode    | 80-001                               |
      | Individual.ShippingAddress.City          | Gdansk                               |
      | Individual.ShippingAddress.Street        | Portowa                              |
      | Individual.ShippingAddress.BuildingNumber | 4A                                   |
      | Individual.ShippingAddress.ApartmentNumber | 2                                   |
    When I submit the missing customer update individual request
    Then the update individual data request fails with a not found response
      | Field      | Value                                                                                                                                             |
      | StatusCode | 404                                                                                                                                               |
      | Title      | Resource not found.                                                                                                                               |
      | Detail     | Resource Customer identified by id 33333333-3333-3333-3333-333333333333 cannot be found in database during action UpdateIndividualData.           |
      | Type       | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4                                                                                       |
      | Instance   | /customers/33333333-3333-3333-3333-333333333333/individual                                                                                       |
      | HasTraceId | true                                                                                                                                              |

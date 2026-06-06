@allure.description:Ensures_adding_a_company_to_a_missing_customer_returns_a_not_found_problem_response.
Feature: Add customer company not found

  Scenario: Add company to missing customer returns not found
    Given I have an add company request for a missing customer
      | Field                   | Value                                |
      | CustomerId              | 11111111-1111-1111-1111-111111111111 |
      | TaxId                   | 1234567890                           |
      | CompanyName             | Missing Customer Company             |
      | BillingPostalCode       | 00-950                               |
      | BillingCity             | Warsaw                               |
      | BillingStreet           | Prosta                               |
      | BillingBuildingNumber   | 20                                   |
      | BillingApartmentNumber  | 15                                   |
      | ShippingPostalCode      | 80-001                               |
      | ShippingCity            | Gdansk                               |
      | ShippingStreet          | Portowa                              |
      | ShippingBuildingNumber  | 4A                                   |
      | ShippingApartmentNumber | 2                                    |
    When I submit the add company request for the missing customer
    Then the add company request fails with a not found response
      | Field      | Value                                                                                                                                                |
      | StatusCode | 404                                                                                                                                                  |
      | Title      | Resource not found.                                                                                                                                  |
      | Detail     | Resource Customer identified by id 11111111-1111-1111-1111-111111111111 cannot be found in database during action AddCompany.                       |
      | Type       | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4                                                                                          |
      | Instance   | /customers/11111111-1111-1111-1111-111111111111/companies                                                                                           |
      | HasTraceId | true                                                                                                                                                 |

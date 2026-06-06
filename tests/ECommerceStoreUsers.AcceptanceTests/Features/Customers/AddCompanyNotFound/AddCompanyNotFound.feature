@allure.description:Ensures_adding_a_company_to_a_missing_customer_by_customer_id_returns_a_not_found_problem_details_response.
Feature: Add company not found

  Scenario: Add company by missing customer id returns not found response
    Given I have a missing customer add company request
      | Field                   | Value                                |
      | CustomerId              | 22222222-2222-2222-2222-222222222222 |
      | TaxId                   | 1234567890                           |
      | CompanyName             | Missing Customer Company Sp. z o.o.  |
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
    When I submit the missing customer add company request
    Then the add company request fails with a not found response
      | Field      | Value                                                                                                                                  |
      | StatusCode | 404                                                                                                                                    |
      | Title      | Resource not found.                                                                                                                    |
      | Detail     | Resource Customer identified by id 22222222-2222-2222-2222-222222222222 cannot be found in database during action AddCompany.          |
      | Type       | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4                                                                            |
      | Instance   | /customers/22222222-2222-2222-2222-222222222222/companies                                                                              |
      | HasTraceId | true                                                                                                                                   |

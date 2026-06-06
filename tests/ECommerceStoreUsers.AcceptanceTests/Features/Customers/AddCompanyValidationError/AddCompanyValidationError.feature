@allure.description:Ensures_adding_a_company_with_empty_customer_id_returns_a_bad_request_with_validation_errors.
Feature: Add customer company validation

  Scenario: Add company with empty customer id returns bad request
    Given I have an invalid add company request
      | Field                   | Value                                |
      | CustomerId              | 00000000-0000-0000-0000-000000000000 |
      | TaxId                   | 1234567890                           |
      | CompanyName             | Validation Company                   |
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
    When I submit the add company request for validation
    Then the add company request fails with a validation error
      | Field      | Value                       |
      | StatusCode | 400                         |
      | Message    | Guid cannot be empty.       |
      | Name       | EmptyGuidRule               |
      | Entity     | Guid                        |

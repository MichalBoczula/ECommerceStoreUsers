@allure.description:Ensures_adding_a_company_with_invalid_company_data_to_an_existing_customer_returns_a_bad_request_with_validation_errors.
Feature: Add customer company validation

  Scenario: Add company with invalid tax id returns bad request
    Given a customer exists for add company validation request
      | Field                           | Value                           |
      | ExternalId                      | customer-company-validation-123 |
      | FirstName                       | Company                         |
      | LastName                        | Validation                      |
      | Email                           | company.validation@db.com       |
      | Phone                           | 123456789                       |
      | BillingPostalCode               | 00-001                          |
      | BillingCity                     | Warsaw                          |
      | BillingStreet                   | Marszalkowska                   |
      | BillingBuildingNumber           | 10                              |
      | BillingApartmentNumber          | 5                               |
      | ShippingPostalCode              | 30-002                          |
      | ShippingCity                    | Krakow                          |
      | ShippingStreet                  | Dluga                           |
      | ShippingBuildingNumber          | 12                              |
      | ShippingApartmentNumber         | 7                               |
    And I have an invalid add company request
      | Field                           | Value                           |
      | TaxId                           | invalid-tax-id                  |
      | CompanyName                     | Invalid Tax Company Sp. z o.o.  |
      | BillingPostalCode               | 00-950                          |
      | BillingCity                     | Warsaw                          |
      | BillingStreet                   | Prosta                          |
      | BillingBuildingNumber           | 20                              |
      | BillingApartmentNumber          | 15                              |
      | ShippingPostalCode              | 80-001                          |
      | ShippingCity                    | Gdansk                          |
      | ShippingStreet                  | Portowa                         |
      | ShippingBuildingNumber          | 4A                              |
      | ShippingApartmentNumber         | 2                               |
    When I submit the add company request for validation
    Then adding the company fails with a validation error
      | Field                           | Value                                                        |
      | StatusCode                      | 400                                                          |
      | Message                         | Tax Id must be a valid Polish NIP containing exactly 10 digits. |
      | Name                            | CompanyDataTaxIdValidationRule                               |
      | Entity                          | CompanyData                                                  |

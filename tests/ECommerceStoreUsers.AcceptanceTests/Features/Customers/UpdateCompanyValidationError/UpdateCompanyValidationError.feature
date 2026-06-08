@allure.description:Ensures_updating_an_existing_customer_company_with_an_empty_company_name_returns_a_bad_request_with_validation_errors.
Feature: Update customer company validation

  Scenario: Update existing customer company with empty company name returns bad request
    Given a customer with company exists for update company validation request
      | Field                           | Value                                   |
      | ExternalId                      | customer-update-company-validation-123  |
      | FirstName                       | Update                                  |
      | LastName                        | Company                                 |
      | Email                           | update.company@db.com                   |
      | Phone                           | 123456789                               |
      | BillingPostalCode               | 00-001                                  |
      | BillingCity                     | Warsaw                                  |
      | BillingStreet                   | Marszalkowska                           |
      | BillingBuildingNumber           | 10                                      |
      | BillingApartmentNumber          | 5                                       |
      | ShippingPostalCode              | 30-002                                  |
      | ShippingCity                    | Krakow                                  |
      | ShippingStreet                  | Dluga                                   |
      | ShippingBuildingNumber          | 12                                      |
      | ShippingApartmentNumber         | 7                                       |
      | CompanyTaxId                    | 1234567890                              |
      | CompanyName                     | Existing Company Sp. z o.o.             |
      | CompanyBillingPostalCode        | 00-950                                  |
      | CompanyBillingCity              | Warsaw                                  |
      | CompanyBillingStreet            | Prosta                                  |
      | CompanyBillingBuildingNumber    | 20                                      |
      | CompanyBillingApartmentNumber   | 15                                      |
      | CompanyShippingPostalCode       | 80-001                                  |
      | CompanyShippingCity             | Gdansk                                  |
      | CompanyShippingStreet           | Portowa                                 |
      | CompanyShippingBuildingNumber   | 4A                                      |
      | CompanyShippingApartmentNumber  | 2                                       |
    And I have an invalid update company request
      | Field                           | Value                                   |
      | TaxId                           | 1234567890                              |
      | CompanyName                     |                                         |
      | BillingPostalCode               | 00-950                                  |
      | BillingCity                     | Warsaw                                  |
      | BillingStreet                   | Prosta                                  |
      | BillingBuildingNumber           | 20                                      |
      | BillingApartmentNumber          | 15                                      |
      | ShippingPostalCode              | 80-001                                  |
      | ShippingCity                    | Gdansk                                  |
      | ShippingStreet                  | Portowa                                 |
      | ShippingBuildingNumber          | 4A                                      |
      | ShippingApartmentNumber         | 2                                       |
    When I submit the update company request for validation
    Then updating the company fails with a validation error
      | Field                           | Value                                   |
      | StatusCode                      | 400                                     |
      | Message                         | Company Name cannot be empty or white space. |
      | Name                            | CompanyDataCompanyNameValidationRule    |
      | Entity                          | CompanyData                             |

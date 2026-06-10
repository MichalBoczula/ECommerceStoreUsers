@allure.description:Ensures_updating_existing_customer_company_data_with_invalid_update_company_contract_values_returns_a_bad_request_with_validation_errors.
Feature: Update customer company data validation

  Scenario: Update existing customer company data with invalid tax id and company name returns bad request
    Given a customer with a company exists for update company validation request
      | Field                           | Value                                  |
      | ExternalId                      | customer-update-company-validation-123 |
      | FirstName                       | Existing                               |
      | LastName                        | CompanyOwner                           |
      | Email                           | update.company.validation@db.com       |
      | Phone                           | 111222333                              |
      | BillingPostalCode               | 00-001                                 |
      | BillingCity                     | Warsaw                                 |
      | BillingStreet                   | Main Street                            |
      | BillingBuildingNumber           | 10                                     |
      | BillingApartmentNumber          | 20                                     |
      | ShippingPostalCode              | 00-002                                 |
      | ShippingCity                    | Krakow                                 |
      | ShippingStreet                  | Shipping Street                        |
      | ShippingBuildingNumber          | 15                                     |
      | ShippingApartmentNumber         | 25                                     |
      | CompanyTaxId                    | 1234567890                             |
      | CompanyName                     | Existing Company Sp. z o.o.            |
      | CompanyBillingPostalCode        | 10-100                                 |
      | CompanyBillingCity              | Poznan                                 |
      | CompanyBillingStreet            | Company Billing Street                 |
      | CompanyBillingBuildingNumber    | 5                                      |
      | CompanyBillingApartmentNumber   | 11                                     |
      | CompanyShippingPostalCode       | 20-200                                 |
      | CompanyShippingCity             | Wroclaw                                |
      | CompanyShippingStreet           | Company Shipping Street                |
      | CompanyShippingBuildingNumber   | 7B                                     |
      | CompanyShippingApartmentNumber  | 14                                     |
    And I have an invalid update company request
      | Field                           | Value                  |
      | TaxId                           | invalid-tax-id         |
      | CompanyName                     |                        |
      | BillingPostalCode               | 30-300                 |
      | BillingCity                     | Gdansk                 |
      | BillingStreet                   | Updated Billing Street |
      | BillingBuildingNumber           | 22                     |
      | BillingApartmentNumber          | 3                      |
      | ShippingPostalCode              | 40-400                 |
      | ShippingCity                    | Lodz                   |
      | ShippingStreet                  | Updated Shipping Street |
      | ShippingBuildingNumber          | 44                     |
      | ShippingApartmentNumber         | 8                      |
    When I submit the update company request for validation
    Then updating the company data fails with validation errors
      | Field                           | Value                                                        |
      | StatusCode                      | 400                                                          |
      | TaxIdMessage                    | Tax Id must be a valid Polish NIP containing exactly 10 digits. |
      | TaxIdName                       | CompanyDataTaxIdValidationRule                               |
      | TaxIdEntity                     | CompanyData                                                  |
      | CompanyNameMessage              | Company Name cannot be empty or white space.                 |
      | CompanyNameName                 | CompanyDataCompanyNameValidationRule                         |
      | CompanyNameEntity               | CompanyData                                                  |

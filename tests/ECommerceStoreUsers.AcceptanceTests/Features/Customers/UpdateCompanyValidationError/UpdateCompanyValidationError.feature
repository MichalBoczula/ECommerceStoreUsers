@allure.description:Ensures_updating_an_existing_customer_company_with_an_empty_company_name_returns_a_bad_request_with_validation_errors.
Feature: Update customer company validation

  Scenario: Update existing customer company with empty company name returns bad request
    Given a customer with company exists for update company validation request
      | Field                                      | Value                              |
      | ExternalId                                 | customer-update-company-validation |
      | Individual.FirstName                       | Company                            |
      | Individual.LastName                        | Validation                         |
      | Individual.Email                           | company.validation@example.com     |
      | Individual.Phone                           | 123456789                          |
      | Individual.BillingAddress.PostalCode       | 00-001                             |
      | Individual.BillingAddress.City             | Warsaw                             |
      | Individual.BillingAddress.Street           | Marszalkowska                      |
      | Individual.BillingAddress.BuildingNumber   | 10                                 |
      | Individual.BillingAddress.ApartmentNumber  | 5                                  |
      | Individual.ShippingAddress.PostalCode      | 30-002                             |
      | Individual.ShippingAddress.City            | Krakow                             |
      | Individual.ShippingAddress.Street          | Dluga                              |
      | Individual.ShippingAddress.BuildingNumber  | 12                                 |
      | Individual.ShippingAddress.ApartmentNumber | 7                                  |
      | Company.TaxId                              | 1234567890                         |
      | Company.CompanyName                        | Example Validation Company         |
      | Company.BillingAddress.PostalCode          | 00-950                             |
      | Company.BillingAddress.City                | Warsaw                             |
      | Company.BillingAddress.Street              | Prosta                             |
      | Company.BillingAddress.BuildingNumber      | 20                                 |
      | Company.BillingAddress.ApartmentNumber     | 15                                 |
      | Company.ShippingAddress.PostalCode         | 80-001                             |
      | Company.ShippingAddress.City               | Gdansk                             |
      | Company.ShippingAddress.Street             | Portowa                            |
      | Company.ShippingAddress.BuildingNumber     | 4A                                 |
      | Company.ShippingAddress.ApartmentNumber    | 2                                  |
    And I have an invalid update company request
      | Field                                   | Value      |
      | Company.TaxId                           | 9876543210 |
      | Company.CompanyName                     |            |
      | Company.BillingAddress.PostalCode       | 01-234     |
      | Company.BillingAddress.City             | Poznan     |
      | Company.BillingAddress.Street           | Nowa       |
      | Company.BillingAddress.BuildingNumber   | 30         |
      | Company.BillingAddress.ApartmentNumber  | 8          |
      | Company.ShippingAddress.PostalCode      | 70-001     |
      | Company.ShippingAddress.City            | Szczecin   |
      | Company.ShippingAddress.Street          | Morska     |
      | Company.ShippingAddress.BuildingNumber  | 9B         |
      | Company.ShippingAddress.ApartmentNumber | 11         |
    When I submit the update company request for validation
    Then updating the company fails with a validation error
      | Field      | Value                                        |
      | StatusCode | 400                                          |
      | Message    | Company Name cannot be empty or white space. |
      | Name       | CompanyDataCompanyNameValidationRule         |
      | Entity     | CompanyData                                  |

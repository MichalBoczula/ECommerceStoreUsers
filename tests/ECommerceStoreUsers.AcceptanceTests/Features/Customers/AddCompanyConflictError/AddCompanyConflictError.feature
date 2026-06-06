@allure.description:Ensures_adding_a_company_with_a_tax_id_that_already_exists_for_the_customer_returns_a_conflict_response.
Feature: Add company conflict

  Scenario: Add duplicate company tax id to existing customer returns conflict response
    Given a customer with a company exists for add company conflict request
      | Field                                           | Value                                |
      | ExternalId                                      | customer-add-company-conflict-123456 |
      | Individual.FirstName                            | Alice                                |
      | Individual.LastName                             | CompanyConflict                      |
      | Individual.Email                                | alice.company.conflict@example.com   |
      | Individual.Phone                                | 555123456                            |
      | Individual.BillingAddress.PostalCode            | 01-100                               |
      | Individual.BillingAddress.City                  | Warsaw                               |
      | Individual.BillingAddress.Street                | Existing Customer Street             |
      | Individual.BillingAddress.BuildingNumber        | 7                                    |
      | Individual.BillingAddress.ApartmentNumber       | 14                                   |
      | Individual.ShippingAddress.PostalCode           | 31-200                               |
      | Individual.ShippingAddress.City                 | Krakow                               |
      | Individual.ShippingAddress.Street               | Existing Customer Avenue             |
      | Individual.ShippingAddress.BuildingNumber       | 9                                    |
      | Individual.ShippingAddress.ApartmentNumber      | 18                                   |
      | ExistingCompany.TaxId                           | 9988776655                           |
      | ExistingCompany.CompanyName                     | Existing Conflict Company Sp. z o.o. |
      | ExistingCompany.BillingAddress.PostalCode       | 00-950                               |
      | ExistingCompany.BillingAddress.City             | Warsaw                               |
      | ExistingCompany.BillingAddress.Street           | Existing Company Street              |
      | ExistingCompany.BillingAddress.BuildingNumber   | 20                                   |
      | ExistingCompany.BillingAddress.ApartmentNumber  | 15                                   |
      | ExistingCompany.ShippingAddress.PostalCode      | 80-001                               |
      | ExistingCompany.ShippingAddress.City            | Gdansk                               |
      | ExistingCompany.ShippingAddress.Street          | Existing Company Portowa             |
      | ExistingCompany.ShippingAddress.BuildingNumber  | 4A                                   |
      | ExistingCompany.ShippingAddress.ApartmentNumber | 2                                    |
    And I have a duplicate tax id add company request
      | Field                                     | Value                                 |
      | TaxId                                     | 9988776655                            |
      | CompanyName                               | Duplicate Conflict Company Sp. z o.o. |
      | BillingAddress.PostalCode                 | 02-300                                |
      | BillingAddress.City                       | Lodz                                  |
      | BillingAddress.Street                     | Duplicate Billing Street              |
      | BillingAddress.BuildingNumber             | 42                                    |
      | BillingAddress.ApartmentNumber            | 8                                     |
      | ShippingAddress.PostalCode                | 61-400                                |
      | ShippingAddress.City                      | Poznan                                |
      | ShippingAddress.Street                    | Duplicate Shipping Street             |
      | ShippingAddress.BuildingNumber            | 11B                                   |
      | ShippingAddress.ApartmentNumber           | 6                                     |
    When I submit the duplicate tax id add company request
    Then the add company request fails with a conflict error
      | Field           | Value                                                                                       |
      | StatusCode      | 409                                                                                         |
      | Status          | 409                                                                                         |
      | Title           | Conflict.                                                                                   |
      | Type            | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8                                |
      | DetailContains  | Resource CompanyData identified by id 9988776655 already exists in db. Error in action AddCompany. |
      | DuplicatedTaxId | 9988776655                                                                                  |
      | HasTraceId      | true                                                                                        |

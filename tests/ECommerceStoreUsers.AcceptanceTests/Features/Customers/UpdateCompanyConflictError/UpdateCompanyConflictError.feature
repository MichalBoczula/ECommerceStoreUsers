@allure.description:Ensures_updating_a_company_with_a_tax_id_that_is_already_used_by_another_company_for_the_customer_returns_a_conflict_response.
Feature: Update company conflict

  Scenario: Update company tax id to another existing company tax id returns conflict response
    Given a customer with two companies exists for update company conflict request
      | Field                                           | Value                                      |
      | ExternalId                                      | customer-update-company-conflict-123456    |
      | Individual.FirstName                            | Alice                                      |
      | Individual.LastName                             | UpdateCompanyConflict                      |
      | Individual.Email                                | alice.update.company.conflict@example.com  |
      | Individual.Phone                                | 555123456                                  |
      | Individual.BillingAddress.PostalCode            | 01-100                                     |
      | Individual.BillingAddress.City                  | Warsaw                                     |
      | Individual.BillingAddress.Street                | Existing Customer Street                   |
      | Individual.BillingAddress.BuildingNumber        | 7                                          |
      | Individual.BillingAddress.ApartmentNumber       | 14                                         |
      | Individual.ShippingAddress.PostalCode           | 31-200                                     |
      | Individual.ShippingAddress.City                 | Krakow                                     |
      | Individual.ShippingAddress.Street               | Existing Customer Avenue                   |
      | Individual.ShippingAddress.BuildingNumber       | 9                                          |
      | Individual.ShippingAddress.ApartmentNumber      | 18                                         |
      | FirstCompany.TaxId                              | 9988776655                                 |
      | FirstCompany.CompanyName                        | First Conflict Company Sp. z o.o.          |
      | FirstCompany.BillingAddress.PostalCode          | 00-950                                     |
      | FirstCompany.BillingAddress.City                | Warsaw                                     |
      | FirstCompany.BillingAddress.Street              | First Company Street                       |
      | FirstCompany.BillingAddress.BuildingNumber      | 20                                         |
      | FirstCompany.BillingAddress.ApartmentNumber     | 15                                         |
      | FirstCompany.ShippingAddress.PostalCode         | 80-001                                     |
      | FirstCompany.ShippingAddress.City               | Gdansk                                     |
      | FirstCompany.ShippingAddress.Street             | First Company Portowa                      |
      | FirstCompany.ShippingAddress.BuildingNumber     | 4A                                         |
      | FirstCompany.ShippingAddress.ApartmentNumber    | 2                                          |
      | SecondCompany.TaxId                             | 1122334455                                 |
      | SecondCompany.CompanyName                       | Second Conflict Company Sp. z o.o.         |
      | SecondCompany.BillingAddress.PostalCode         | 02-300                                     |
      | SecondCompany.BillingAddress.City               | Lodz                                       |
      | SecondCompany.BillingAddress.Street             | Second Company Street                      |
      | SecondCompany.BillingAddress.BuildingNumber     | 42                                         |
      | SecondCompany.BillingAddress.ApartmentNumber    | 8                                          |
      | SecondCompany.ShippingAddress.PostalCode        | 61-400                                     |
      | SecondCompany.ShippingAddress.City              | Poznan                                     |
      | SecondCompany.ShippingAddress.Street            | Second Company Shipping Street             |
      | SecondCompany.ShippingAddress.BuildingNumber    | 11B                                        |
      | SecondCompany.ShippingAddress.ApartmentNumber   | 6                                          |
      | CompanyToUpdate                                 | SecondCompany                              |
    And I have a duplicate tax id update company request
      | Field                           | Value                                   |
      | TaxId                           | 9988776655                              |
      | CompanyName                     | Updated Duplicate Tax Company Sp. z o.o. |
      | BillingAddress.PostalCode       | 03-300                                  |
      | BillingAddress.City             | Wroclaw                                 |
      | BillingAddress.Street           | Updated Billing Street                  |
      | BillingAddress.BuildingNumber   | 33                                      |
      | BillingAddress.ApartmentNumber  | 12                                      |
      | ShippingAddress.PostalCode      | 70-400                                  |
      | ShippingAddress.City            | Szczecin                                |
      | ShippingAddress.Street          | Updated Shipping Street                 |
      | ShippingAddress.BuildingNumber  | 22C                                     |
      | ShippingAddress.ApartmentNumber | 9                                       |
    When I submit the duplicate tax id update company request
    Then the update company request fails with a conflict error
      | Field           | Value                                                                                                |
      | StatusCode      | 409                                                                                                  |
      | Status          | 409                                                                                                  |
      | Title           | Conflict.                                                                                            |
      | Type            | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8                                         |
      | DetailContains  | Resource CompanyData identified by id 9988776655 already exists in db. Error in action UpdateCompany. |
      | DuplicatedTaxId | 9988776655                                                                                           |
      | HasTraceId      | true                                                                                                 |

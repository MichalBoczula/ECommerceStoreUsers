@allure.description:Ensures_updating_an_existing_customer_company_returns_a_success_response_with_the_updated_company_payload_fields.
Feature: Update customer company

  @allure.label.scenarioId:REF07_UpdateCompany_04
  Scenario: Update customer company returns updated customer data
    An existing customer changes the details of an identified company.
    The response contains the revised company data within the customer profile.
    Given a customer with company exists for update company request
      | Field                                      | Value                          |
      | ExternalId                                 | customer-update-company-123456 |
      | Individual.FirstName                       | Company                        |
      | Individual.LastName                        | Owner                          |
      | Individual.Email                           | company.owner@example.com      |
      | Individual.Phone                           | 123456789                      |
      | Individual.BillingAddress.PostalCode       | 00-001                         |
      | Individual.BillingAddress.City             | Warsaw                         |
      | Individual.BillingAddress.Street           | Marszalkowska                  |
      | Individual.BillingAddress.BuildingNumber   | 10                             |
      | Individual.BillingAddress.ApartmentNumber  | 5                              |
      | Individual.ShippingAddress.PostalCode      | 30-002                         |
      | Individual.ShippingAddress.City            | Krakow                         |
      | Individual.ShippingAddress.Street          | Dluga                          |
      | Individual.ShippingAddress.BuildingNumber  | 12                             |
      | Individual.ShippingAddress.ApartmentNumber | 7                              |
      | Company.TaxId                              | 1234567890                     |
      | Company.CompanyName                        | Example Company Sp. z o.o.     |
      | Company.BillingAddress.PostalCode          | 00-950                         |
      | Company.BillingAddress.City                | Warsaw                         |
      | Company.BillingAddress.Street              | Prosta                         |
      | Company.BillingAddress.BuildingNumber      | 20                             |
      | Company.BillingAddress.ApartmentNumber     | 15                             |
      | Company.ShippingAddress.PostalCode         | 80-001                         |
      | Company.ShippingAddress.City               | Gdansk                         |
      | Company.ShippingAddress.Street             | Portowa                        |
      | Company.ShippingAddress.BuildingNumber     | 4A                             |
      | Company.ShippingAddress.ApartmentNumber    | 2                              |
    And I have a valid update company request
      | Field                                   | Value                        |
      | Company.TaxId                           | 9876543210                   |
      | Company.CompanyName                     | Updated Example Company S.A. |
      | Company.BillingAddress.PostalCode       | 01-234                       |
      | Company.BillingAddress.City             | Poznan                       |
      | Company.BillingAddress.Street           | Nowa                         |
      | Company.BillingAddress.BuildingNumber   | 30                           |
      | Company.BillingAddress.ApartmentNumber  | 8                            |
      | Company.ShippingAddress.PostalCode      | 70-001                       |
      | Company.ShippingAddress.City            | Szczecin                     |
      | Company.ShippingAddress.Street          | Morska                       |
      | Company.ShippingAddress.BuildingNumber  | 9B                           |
      | Company.ShippingAddress.ApartmentNumber | 11                           |
    When I submit the update company request
    Then the company is updated successfully
      | Field                                      | Value                          |
      | StatusCode                                 | 200                            |
      | HasId                                      | true                           |
      | HasUpdatedAt                               | true                           |
      | ExternalId                                 | customer-update-company-123456 |
      | CompaniesCount                             | 1                              |
      | Individual.FirstName                       | Company                        |
      | Individual.LastName                        | Owner                          |
      | Individual.Email                           | company.owner@example.com      |
      | Individual.Phone                           | 123456789                      |
      | Individual.BillingAddress.PostalCode       | 00-001                         |
      | Individual.BillingAddress.City             | Warsaw                         |
      | Individual.BillingAddress.Street           | Marszalkowska                  |
      | Individual.BillingAddress.BuildingNumber   | 10                             |
      | Individual.BillingAddress.ApartmentNumber  | 5                              |
      | Individual.ShippingAddress.PostalCode      | 30-002                         |
      | Individual.ShippingAddress.City            | Krakow                         |
      | Individual.ShippingAddress.Street          | Dluga                          |
      | Individual.ShippingAddress.BuildingNumber  | 12                             |
      | Individual.ShippingAddress.ApartmentNumber | 7                              |
      | Company.TaxId                              | 9876543210                     |
      | Company.CompanyName                        | Updated Example Company S.A.   |
      | Company.BillingAddress.PostalCode          | 01-234                         |
      | Company.BillingAddress.City                | Poznan                         |
      | Company.BillingAddress.Street              | Nowa                           |
      | Company.BillingAddress.BuildingNumber      | 30                             |
      | Company.BillingAddress.ApartmentNumber     | 8                              |
      | Company.ShippingAddress.PostalCode         | 70-001                         |
      | Company.ShippingAddress.City               | Szczecin                       |
      | Company.ShippingAddress.Street             | Morska                         |
      | Company.ShippingAddress.BuildingNumber     | 9B                             |
      | Company.ShippingAddress.ApartmentNumber    | 11                             |
    And the customer database contains the expected current and history snapshots
      | Field          | Value         |
      | CurrentBefore  | 1             |
      | HistoryBefore  | 1             |
      | VersionBefore  | 0             |
      | CurrentAfter   | 1             |
      | HistoryAfter   | 2             |
      | VersionAfter   | 1             |
      | HistoryActions | Insert,Update |
      | UpdatedAt      | retained      |

  @allure.label.scenarioId:REF03_UpdateCompany_PreservesOtherCompany
  Scenario: Update one company preserves the other company and individual data
    A customer has two companies and changes the selected company.
    Both company identifiers and all unselected company and individual fields are retained in current data and history.
    Given a customer with company exists for update company request
      | Field                                      | Value                          |
      | ExternalId                                 | customer-update-company-123456 |
      | Individual.FirstName                       | Company                        |
      | Individual.LastName                        | Owner                          |
      | Individual.Email                           | company.owner@example.com      |
      | Individual.Phone                           | 123456789                      |
      | Individual.BillingAddress.PostalCode       | 00-001                         |
      | Individual.BillingAddress.City             | Warsaw                         |
      | Individual.BillingAddress.Street           | Marszalkowska                  |
      | Individual.BillingAddress.BuildingNumber   | 10                             |
      | Individual.BillingAddress.ApartmentNumber  | 5                              |
      | Individual.ShippingAddress.PostalCode      | 30-002                         |
      | Individual.ShippingAddress.City            | Krakow                         |
      | Individual.ShippingAddress.Street          | Dluga                          |
      | Individual.ShippingAddress.BuildingNumber  | 12                             |
      | Individual.ShippingAddress.ApartmentNumber | 7                              |
      | Company.TaxId                              | 1234567890                     |
      | Company.CompanyName                        | Example Company Sp. z o.o.     |
      | Company.BillingAddress.PostalCode          | 00-950                         |
      | Company.BillingAddress.City                | Warsaw                         |
      | Company.BillingAddress.Street              | Prosta                         |
      | Company.BillingAddress.BuildingNumber      | 20                             |
      | Company.BillingAddress.ApartmentNumber     | 15                             |
      | Company.ShippingAddress.PostalCode         | 80-001                         |
      | Company.ShippingAddress.City               | Gdansk                         |
      | Company.ShippingAddress.Street             | Portowa                        |
      | Company.ShippingAddress.BuildingNumber     | 4A                             |
      | Company.ShippingAddress.ApartmentNumber    | 2                              |
      | Company2.TaxId                             | 5555555555                     |
      | Company2.CompanyName                       | Retained Company               |
      | Company2.BillingAddress.PostalCode         | 50-001                         |
      | Company2.BillingAddress.City               | Wroclaw                        |
      | Company2.BillingAddress.Street             | Retained Street                |
      | Company2.BillingAddress.BuildingNumber     | 42                             |
      | Company2.BillingAddress.ApartmentNumber    | <null>                         |
      | Company2.ShippingAddress.PostalCode        | 50-001                         |
      | Company2.ShippingAddress.City              | Wroclaw                        |
      | Company2.ShippingAddress.Street            | Retained Street                |
      | Company2.ShippingAddress.BuildingNumber    | 42                             |
      | Company2.ShippingAddress.ApartmentNumber   | <null>                         |
    And I have a valid update company request
      | Field                                   | Value                        |
      | Company.TaxId                           | 9876543210                   |
      | Company.CompanyName                     | Updated Example Company S.A. |
      | Company.BillingAddress.PostalCode       | 01-234                       |
      | Company.BillingAddress.City             | Poznan                       |
      | Company.BillingAddress.Street           | Nowa                         |
      | Company.BillingAddress.BuildingNumber   | 30                           |
      | Company.BillingAddress.ApartmentNumber  | 8                            |
      | Company.ShippingAddress.PostalCode      | 70-001                       |
      | Company.ShippingAddress.City            | Szczecin                     |
      | Company.ShippingAddress.Street          | Morska                       |
      | Company.ShippingAddress.BuildingNumber  | 9B                           |
      | Company.ShippingAddress.ApartmentNumber | 11                           |
    When I submit the update company request
    Then the company is updated successfully
      | Field                                      | Value                          |
      | StatusCode                                 | 200                            |
      | HasId                                      | true                           |
      | HasUpdatedAt                               | true                           |
      | ExternalId                                 | customer-update-company-123456 |
      | CompaniesCount                             | 2                              |
      | Individual.FirstName                       | Company                        |
      | Individual.LastName                        | Owner                          |
      | Individual.Email                           | company.owner@example.com      |
      | Individual.Phone                           | 123456789                      |
      | Individual.BillingAddress.PostalCode       | 00-001                         |
      | Individual.BillingAddress.City             | Warsaw                         |
      | Individual.BillingAddress.Street           | Marszalkowska                  |
      | Individual.BillingAddress.BuildingNumber   | 10                             |
      | Individual.BillingAddress.ApartmentNumber  | 5                              |
      | Individual.ShippingAddress.PostalCode      | 30-002                         |
      | Individual.ShippingAddress.City            | Krakow                         |
      | Individual.ShippingAddress.Street          | Dluga                          |
      | Individual.ShippingAddress.BuildingNumber  | 12                             |
      | Individual.ShippingAddress.ApartmentNumber | 7                              |
      | Company.TaxId                              | 9876543210                     |
      | Company.CompanyName                        | Updated Example Company S.A.   |
      | Company.BillingAddress.PostalCode          | 01-234                         |
      | Company.BillingAddress.City                | Poznan                         |
      | Company.BillingAddress.Street              | Nowa                           |
      | Company.BillingAddress.BuildingNumber      | 30                             |
      | Company.BillingAddress.ApartmentNumber     | 8                              |
      | Company.ShippingAddress.PostalCode         | 70-001                         |
      | Company.ShippingAddress.City               | Szczecin                       |
      | Company.ShippingAddress.Street             | Morska                         |
      | Company.ShippingAddress.BuildingNumber     | 9B                             |
      | Company.ShippingAddress.ApartmentNumber    | 11                             |
      | Company2.TaxId                             | 5555555555                     |
      | Company2.CompanyName                       | Retained Company               |
      | Company2.BillingAddress.PostalCode         | 50-001                         |
      | Company2.BillingAddress.City               | Wroclaw                        |
      | Company2.BillingAddress.Street             | Retained Street                |
      | Company2.BillingAddress.BuildingNumber     | 42                             |
      | Company2.BillingAddress.ApartmentNumber    | <null>                         |
      | Company2.ShippingAddress.PostalCode        | 50-001                         |
      | Company2.ShippingAddress.City              | Wroclaw                        |
      | Company2.ShippingAddress.Street            | Retained Street                |
      | Company2.ShippingAddress.BuildingNumber    | 42                             |
      | Company2.ShippingAddress.ApartmentNumber   | <null>                         |
    And the customer database contains the expected current and history snapshots
      | Field          | Value         |
      | CurrentBefore  | 1             |
      | HistoryBefore  | 1             |
      | VersionBefore  | 0             |
      | CurrentAfter   | 1             |
      | HistoryAfter   | 2             |
      | VersionAfter   | 1             |
      | HistoryActions | Insert,Update |
      | UpdatedAt      | retained      |

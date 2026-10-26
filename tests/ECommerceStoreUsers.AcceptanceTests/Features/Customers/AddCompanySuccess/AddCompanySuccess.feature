@allure.description:Ensures_adding_a_company_to_an_existing_customer_returns_a_success_response_with_the_company_payload_fields.
Feature: Add customer company

  @allure.label.scenarioId:REF07_AddCompany_04
  Scenario: Add company to customer returns updated customer data
    An existing customer adds company details with a new tax identifier and addresses.
    The response contains the updated customer and the newly assigned company identifier.
    Given a customer exists for add company request
      | Field                                      | Value                   |
      | ExternalId                                 | customer-company-123456 |
      | Individual.FirstName                       | Company                 |
      | Individual.LastName                        | Client                  |
      | Individual.Email                           | company.client@db.com   |
      | Individual.Phone                           | 123456789               |
      | Individual.BillingAddress.PostalCode       | 00-001                  |
      | Individual.BillingAddress.City             | Warsaw                  |
      | Individual.BillingAddress.Street           | Marszalkowska           |
      | Individual.BillingAddress.BuildingNumber   | 10                      |
      | Individual.BillingAddress.ApartmentNumber  | 5                       |
      | Individual.ShippingAddress.PostalCode      | 30-002                  |
      | Individual.ShippingAddress.City            | Krakow                  |
      | Individual.ShippingAddress.Street          | Dluga                   |
      | Individual.ShippingAddress.BuildingNumber  | 12                      |
      | Individual.ShippingAddress.ApartmentNumber | 7                       |
    And I have a valid add company request
      | Field                                   | Value                      |
      | Company.TaxId                           | 1234567890                 |
      | Company.CompanyName                     | Example Company Sp. z o.o. |
      | Company.BillingAddress.PostalCode       | 00-950                     |
      | Company.BillingAddress.City             | Warsaw                     |
      | Company.BillingAddress.Street           | Prosta                     |
      | Company.BillingAddress.BuildingNumber   | 20                         |
      | Company.BillingAddress.ApartmentNumber  | 15                         |
      | Company.ShippingAddress.PostalCode      | 80-001                     |
      | Company.ShippingAddress.City            | Gdansk                     |
      | Company.ShippingAddress.Street          | Portowa                    |
      | Company.ShippingAddress.BuildingNumber  | 4A                         |
      | Company.ShippingAddress.ApartmentNumber | 2                          |
    When I submit the add company request
    Then the company is added to the customer successfully
      | Field                                      | Value                      |
      | StatusCode                                 | 200                        |
      | HasId                                      | true                       |
      | HasUpdatedAt                               | true                       |
      | ExternalId                                 | customer-company-123456    |
      | CompaniesCount                             | 1                          |
      | Individual.FirstName                       | Company                    |
      | Individual.LastName                        | Client                     |
      | Individual.Email                           | company.client@db.com      |
      | Individual.Phone                           | 123456789                  |
      | Individual.BillingAddress.PostalCode       | 00-001                     |
      | Individual.BillingAddress.City             | Warsaw                     |
      | Individual.BillingAddress.Street           | Marszalkowska              |
      | Individual.BillingAddress.BuildingNumber   | 10                         |
      | Individual.BillingAddress.ApartmentNumber  | 5                          |
      | Individual.ShippingAddress.PostalCode      | 30-002                     |
      | Individual.ShippingAddress.City            | Krakow                     |
      | Individual.ShippingAddress.Street          | Dluga                      |
      | Individual.ShippingAddress.BuildingNumber  | 12                         |
      | Individual.ShippingAddress.ApartmentNumber | 7                          |
      | Company.TaxId                              | 1234567890                 |
      | Company.CompanyName                        | Example Company Sp. z o.o. |
      | Company.BillingAddress.PostalCode          | 00-950                     |
      | Company.BillingAddress.City                | Warsaw                     |
      | Company.BillingAddress.Street              | Prosta                     |
      | Company.BillingAddress.BuildingNumber      | 20                         |
      | Company.BillingAddress.ApartmentNumber     | 15                         |
      | Company.ShippingAddress.PostalCode         | 80-001                     |
      | Company.ShippingAddress.City               | Gdansk                     |
      | Company.ShippingAddress.Street             | Portowa                    |
      | Company.ShippingAddress.BuildingNumber     | 4A                         |
      | Company.ShippingAddress.ApartmentNumber    | 2                          |
    And the customer database contains the expected current and history snapshots
      | Field          | Value         |
      | CurrentBefore  | 1             |
      | HistoryBefore  | 1             |
      | VersionBefore  | 0             |
      | CurrentAfter   | 1             |
      | HistoryAfter   | 2             |
      | VersionAfter   | 1             |
      | HistoryActions | Insert,Update |
      | UpdatedAt      | generated     |

  @allure.label.scenarioId:REF03_AddCompany_PreservesExistingCompany
  Scenario: Add a second company preserves the existing company and individual data
    A customer with a company adds a different company tax identifier and addresses.
    The updated profile and history retain the original company and individual data.
    Given a customer exists for add company request
      | Field                                      | Value                   |
      | ExternalId                                 | customer-company-123456 |
      | Individual.FirstName                       | Company                 |
      | Individual.LastName                        | Client                  |
      | Individual.Email                           | company.client@db.com   |
      | Individual.Phone                           | 123456789               |
      | Individual.BillingAddress.PostalCode       | 00-001                  |
      | Individual.BillingAddress.City             | Warsaw                  |
      | Individual.BillingAddress.Street           | Marszalkowska           |
      | Individual.BillingAddress.BuildingNumber   | 10                      |
      | Individual.BillingAddress.ApartmentNumber  | 5                       |
      | Individual.ShippingAddress.PostalCode      | 30-002                  |
      | Individual.ShippingAddress.City            | Krakow                  |
      | Individual.ShippingAddress.Street          | Dluga                   |
      | Individual.ShippingAddress.BuildingNumber  | 12                      |
      | Individual.ShippingAddress.ApartmentNumber | 7                       |
      | Company.TaxId                              | 5555555555              |
      | Company.CompanyName                        | Retained Company        |
      | Company.BillingAddress.PostalCode          | 50-001                  |
      | Company.BillingAddress.City                | Wroclaw                 |
      | Company.BillingAddress.Street              | Retained Street         |
      | Company.BillingAddress.BuildingNumber      | 42                      |
      | Company.BillingAddress.ApartmentNumber     | <null>                  |
      | Company.ShippingAddress.PostalCode         | 50-001                  |
      | Company.ShippingAddress.City               | Wroclaw                 |
      | Company.ShippingAddress.Street             | Retained Street         |
      | Company.ShippingAddress.BuildingNumber     | 42                      |
      | Company.ShippingAddress.ApartmentNumber    | <null>                  |
    And I have a valid add company request
      | Field                                   | Value                      |
      | Company.TaxId                           | 1234567890                 |
      | Company.CompanyName                     | Example Company Sp. z o.o. |
      | Company.BillingAddress.PostalCode       | 00-950                     |
      | Company.BillingAddress.City             | Warsaw                     |
      | Company.BillingAddress.Street           | Prosta                     |
      | Company.BillingAddress.BuildingNumber   | 20                         |
      | Company.BillingAddress.ApartmentNumber  | 15                         |
      | Company.ShippingAddress.PostalCode      | 80-001                     |
      | Company.ShippingAddress.City            | Gdansk                     |
      | Company.ShippingAddress.Street          | Portowa                    |
      | Company.ShippingAddress.BuildingNumber  | 4A                         |
      | Company.ShippingAddress.ApartmentNumber | 2                          |
    When I submit the add company request
    Then the company is added to the customer successfully
      | Field                                      | Value                      |
      | StatusCode                                 | 200                        |
      | HasId                                      | true                       |
      | HasUpdatedAt                               | true                       |
      | ExternalId                                 | customer-company-123456    |
      | CompaniesCount                             | 2                          |
      | Individual.FirstName                       | Company                    |
      | Individual.LastName                        | Client                     |
      | Individual.Email                           | company.client@db.com      |
      | Individual.Phone                           | 123456789                  |
      | Individual.BillingAddress.PostalCode       | 00-001                     |
      | Individual.BillingAddress.City             | Warsaw                     |
      | Individual.BillingAddress.Street           | Marszalkowska              |
      | Individual.BillingAddress.BuildingNumber   | 10                         |
      | Individual.BillingAddress.ApartmentNumber  | 5                          |
      | Individual.ShippingAddress.PostalCode      | 30-002                     |
      | Individual.ShippingAddress.City            | Krakow                     |
      | Individual.ShippingAddress.Street          | Dluga                      |
      | Individual.ShippingAddress.BuildingNumber  | 12                         |
      | Individual.ShippingAddress.ApartmentNumber | 7                          |
      | Company.TaxId                              | 1234567890                 |
      | Company.CompanyName                        | Example Company Sp. z o.o. |
      | Company.BillingAddress.PostalCode          | 00-950                     |
      | Company.BillingAddress.City                | Warsaw                     |
      | Company.BillingAddress.Street              | Prosta                     |
      | Company.BillingAddress.BuildingNumber      | 20                         |
      | Company.BillingAddress.ApartmentNumber     | 15                         |
      | Company.ShippingAddress.PostalCode         | 80-001                     |
      | Company.ShippingAddress.City               | Gdansk                     |
      | Company.ShippingAddress.Street             | Portowa                    |
      | Company.ShippingAddress.BuildingNumber     | 4A                         |
      | Company.ShippingAddress.ApartmentNumber    | 2                          |
      | Company2.TaxId                             | 5555555555                 |
      | Company2.CompanyName                       | Retained Company           |
      | Company2.BillingAddress.PostalCode         | 50-001                     |
      | Company2.BillingAddress.City               | Wroclaw                    |
      | Company2.BillingAddress.Street             | Retained Street            |
      | Company2.BillingAddress.BuildingNumber     | 42                         |
      | Company2.BillingAddress.ApartmentNumber    | <null>                     |
      | Company2.ShippingAddress.PostalCode        | 50-001                     |
      | Company2.ShippingAddress.City              | Wroclaw                    |
      | Company2.ShippingAddress.Street            | Retained Street            |
      | Company2.ShippingAddress.BuildingNumber    | 42                         |
      | Company2.ShippingAddress.ApartmentNumber   | <null>                     |
    And the customer database contains the expected current and history snapshots
      | Field          | Value         |
      | CurrentBefore  | 1             |
      | HistoryBefore  | 1             |
      | VersionBefore  | 0             |
      | CurrentAfter   | 1             |
      | HistoryAfter   | 2             |
      | VersionAfter   | 1             |
      | HistoryActions | Insert,Update |
      | UpdatedAt      | generated     |

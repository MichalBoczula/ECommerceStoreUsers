@allure.description:Ensures_updating_an_existing_customer_individual_data_returns_a_success_response_with_the_updated_individual_payload_fields.
Feature: Update customer individual data

  @allure.label.scenarioId:REF07_UpdateIndividualData_03
  Scenario: Update customer individual data returns updated customer profile
    An existing customer changes individual and contact details while retaining its identity.
    The update returns the customer profile with the new individual and address values.
    Given a customer exists for update individual request
      | Field                                      | Value                             |
      | ExternalId                                 | customer-update-individual-123456 |
      | Individual.FirstName                       | Alice                             |
      | Individual.LastName                        | Client                            |
      | Individual.Email                           | alice.client@example.com          |
      | Individual.Phone                           | 123456789                         |
      | Individual.BillingAddress.PostalCode       | 00-001                            |
      | Individual.BillingAddress.City             | Warsaw                            |
      | Individual.BillingAddress.Street           | Main Street                       |
      | Individual.BillingAddress.BuildingNumber   | 10                                |
      | Individual.BillingAddress.ApartmentNumber  | 20                                |
      | Individual.ShippingAddress.PostalCode      | 00-002                            |
      | Individual.ShippingAddress.City            | Krakow                            |
      | Individual.ShippingAddress.Street          | Shipping Street                   |
      | Individual.ShippingAddress.BuildingNumber  | 15                                |
      | Individual.ShippingAddress.ApartmentNumber | 25                                |
    And I have a valid update individual request
      | Field                                      | Value                     |
      | Individual.FirstName                       | Alice                     |
      | Individual.LastName                        | Client                    |
      | Individual.Email                           | alice.updated@example.com |
      | Individual.Phone                           | 987654321                 |
      | Individual.BillingAddress.PostalCode       | 00-950                    |
      | Individual.BillingAddress.City             | Warsaw                    |
      | Individual.BillingAddress.Street           | Prosta                    |
      | Individual.BillingAddress.BuildingNumber   | 20                        |
      | Individual.BillingAddress.ApartmentNumber  | 15                        |
      | Individual.ShippingAddress.PostalCode      | 80-001                    |
      | Individual.ShippingAddress.City            | Gdansk                    |
      | Individual.ShippingAddress.Street          | Portowa                   |
      | Individual.ShippingAddress.BuildingNumber  | 4A                        |
      | Individual.ShippingAddress.ApartmentNumber | 2                         |
    When I submit the update individual request
    Then the individual data is updated successfully
      | Field                                      | Value                             |
      | StatusCode                                 | 200                               |
      | HasId                                      | true                              |
      | HasUpdatedAt                               | true                              |
      | ExternalId                                 | customer-update-individual-123456 |
      | CompaniesCount                             | 0                                 |
      | Individual.FirstName                       | Alice                             |
      | Individual.LastName                        | Client                            |
      | Individual.Email                           | alice.updated@example.com         |
      | Individual.Phone                           | 987654321                         |
      | Individual.BillingAddress.PostalCode       | 00-950                            |
      | Individual.BillingAddress.City             | Warsaw                            |
      | Individual.BillingAddress.Street           | Prosta                            |
      | Individual.BillingAddress.BuildingNumber   | 20                                |
      | Individual.BillingAddress.ApartmentNumber  | 15                                |
      | Individual.ShippingAddress.PostalCode      | 80-001                            |
      | Individual.ShippingAddress.City            | Gdansk                            |
      | Individual.ShippingAddress.Street          | Portowa                           |
      | Individual.ShippingAddress.BuildingNumber  | 4A                                |
      | Individual.ShippingAddress.ApartmentNumber | 2                                 |
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

  @allure.label.scenarioId:REF03_UpdateIndividual_PreservesCompany
  Scenario: Update individual data preserves an existing company and nullable apartment values
    A customer with a company changes individual details and clears optional apartment values.
    The returned profile and new history snapshot retain every company field and identifier.
    Given a customer exists for update individual request
      | Field                                      | Value                             |
      | ExternalId                                 | customer-update-individual-123456 |
      | Individual.FirstName                       | Alice                             |
      | Individual.LastName                        | Client                            |
      | Individual.Email                           | alice.client@example.com          |
      | Individual.Phone                           | 123456789                         |
      | Individual.BillingAddress.PostalCode       | 00-001                            |
      | Individual.BillingAddress.City             | Warsaw                            |
      | Individual.BillingAddress.Street           | Main Street                       |
      | Individual.BillingAddress.BuildingNumber   | 10                                |
      | Individual.BillingAddress.ApartmentNumber  | 20                                |
      | Individual.ShippingAddress.PostalCode      | 00-002                            |
      | Individual.ShippingAddress.City            | Krakow                            |
      | Individual.ShippingAddress.Street          | Shipping Street                   |
      | Individual.ShippingAddress.BuildingNumber  | 15                                |
      | Individual.ShippingAddress.ApartmentNumber | 25                                |
      | Company.TaxId                              | 5555555555                        |
      | Company.CompanyName                        | Retained Company                  |
      | Company.BillingAddress.PostalCode          | 50-001                            |
      | Company.BillingAddress.City                | Wroclaw                           |
      | Company.BillingAddress.Street              | Retained Street                   |
      | Company.BillingAddress.BuildingNumber      | 42                                |
      | Company.BillingAddress.ApartmentNumber     | <null>                            |
      | Company.ShippingAddress.PostalCode         | 50-001                            |
      | Company.ShippingAddress.City               | Wroclaw                           |
      | Company.ShippingAddress.Street             | Retained Street                   |
      | Company.ShippingAddress.BuildingNumber     | 42                                |
      | Company.ShippingAddress.ApartmentNumber    | <null>                            |
    And I have a valid update individual request
      | Field                                      | Value                     |
      | Individual.FirstName                       | Alice                     |
      | Individual.LastName                        | Client                    |
      | Individual.Email                           | alice.updated@example.com |
      | Individual.Phone                           | 987654321                 |
      | Individual.BillingAddress.PostalCode       | 00-950                    |
      | Individual.BillingAddress.City             | Warsaw                    |
      | Individual.BillingAddress.Street           | Prosta                    |
      | Individual.BillingAddress.BuildingNumber   | 20                        |
      | Individual.BillingAddress.ApartmentNumber  | <null>                    |
      | Individual.ShippingAddress.PostalCode      | 80-001                    |
      | Individual.ShippingAddress.City            | Gdansk                    |
      | Individual.ShippingAddress.Street          | Portowa                   |
      | Individual.ShippingAddress.BuildingNumber  | 4A                        |
      | Individual.ShippingAddress.ApartmentNumber | <null>                    |
    When I submit the update individual request
    Then the individual data is updated successfully
      | Field                                      | Value                             |
      | StatusCode                                 | 200                               |
      | HasId                                      | true                              |
      | HasUpdatedAt                               | true                              |
      | ExternalId                                 | customer-update-individual-123456 |
      | CompaniesCount                             | 1                                 |
      | Individual.FirstName                       | Alice                             |
      | Individual.LastName                        | Client                            |
      | Individual.Email                           | alice.updated@example.com         |
      | Individual.Phone                           | 987654321                         |
      | Individual.BillingAddress.PostalCode       | 00-950                            |
      | Individual.BillingAddress.City             | Warsaw                            |
      | Individual.BillingAddress.Street           | Prosta                            |
      | Individual.BillingAddress.BuildingNumber   | 20                                |
      | Individual.BillingAddress.ApartmentNumber  | <null>                            |
      | Individual.ShippingAddress.PostalCode      | 80-001                            |
      | Individual.ShippingAddress.City            | Gdansk                            |
      | Individual.ShippingAddress.Street          | Portowa                           |
      | Individual.ShippingAddress.BuildingNumber  | 4A                                |
      | Individual.ShippingAddress.ApartmentNumber | <null>                            |
      | Company.TaxId                              | 5555555555                        |
      | Company.CompanyName                        | Retained Company                  |
      | Company.BillingAddress.PostalCode          | 50-001                            |
      | Company.BillingAddress.City                | Wroclaw                           |
      | Company.BillingAddress.Street              | Retained Street                   |
      | Company.BillingAddress.BuildingNumber      | 42                                |
      | Company.BillingAddress.ApartmentNumber     | <null>                            |
      | Company.ShippingAddress.PostalCode         | 50-001                            |
      | Company.ShippingAddress.City               | Wroclaw                           |
      | Company.ShippingAddress.Street             | Retained Street                   |
      | Company.ShippingAddress.BuildingNumber     | 42                                |
      | Company.ShippingAddress.ApartmentNumber    | <null>                            |
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

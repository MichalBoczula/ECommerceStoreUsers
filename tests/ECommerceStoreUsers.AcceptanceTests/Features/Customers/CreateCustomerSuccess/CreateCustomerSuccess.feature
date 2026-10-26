@allure.description:Ensures_creating_a_customer_profile_returns_a_success_response_with_the_created_individual_and_company_payload_fields.
Feature: Create customer profile

  @allure.label.scenarioId:REF07_CreateCustomer_04
  Scenario: Create customer profile returns created individual and company data
    A new customer registers individual details, billing and shipping addresses, and a company.
    The create request returns the generated customer and company identifiers with the supplied profile data.
    Given I have a valid create customer request
      | Field                                      | Value                      |
      | ExternalId                                 | auth-customer-123456       |
      | Individual.FirstName                       | John                       |
      | Individual.LastName                        | Doe                        |
      | Individual.Email                           | john.doe@example.com       |
      | Individual.Phone                           | 123456789                  |
      | Individual.BillingAddress.PostalCode       | 00-001                     |
      | Individual.BillingAddress.City             | Warsaw                     |
      | Individual.BillingAddress.Street           | Main Street                |
      | Individual.BillingAddress.BuildingNumber   | 10                         |
      | Individual.BillingAddress.ApartmentNumber  | 20                         |
      | Individual.ShippingAddress.PostalCode      | 00-002                     |
      | Individual.ShippingAddress.City            | Krakow                     |
      | Individual.ShippingAddress.Street          | Shipping Street            |
      | Individual.ShippingAddress.BuildingNumber  | 15                         |
      | Individual.ShippingAddress.ApartmentNumber | 25                         |
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
    When I submit the create customer request
    Then the customer profile is created successfully
      | Field                                      | Value                      |
      | StatusCode                                 | 200                        |
      | HasId                                      | true                       |
      | HasUpdatedAt                               | true                       |
      | ExternalId                                 | auth-customer-123456       |
      | CompaniesCount                             | 1                          |
      | Individual.FirstName                       | John                       |
      | Individual.LastName                        | Doe                        |
      | Individual.Email                           | john.doe@example.com       |
      | Individual.Phone                           | 123456789                  |
      | Individual.BillingAddress.PostalCode       | 00-001                     |
      | Individual.BillingAddress.City             | Warsaw                     |
      | Individual.BillingAddress.Street           | Main Street                |
      | Individual.BillingAddress.BuildingNumber   | 10                         |
      | Individual.BillingAddress.ApartmentNumber  | 20                         |
      | Individual.ShippingAddress.PostalCode      | 00-002                     |
      | Individual.ShippingAddress.City            | Krakow                     |
      | Individual.ShippingAddress.Street          | Shipping Street            |
      | Individual.ShippingAddress.BuildingNumber  | 15                         |
      | Individual.ShippingAddress.ApartmentNumber | 25                         |
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
      | Field          | Value     |
      | CurrentBefore  | 0         |
      | HistoryBefore  | 0         |
      | VersionBefore  | none      |
      | CurrentAfter   | 1         |
      | HistoryAfter   | 1         |
      | VersionAfter   | 0         |
      | HistoryActions | Insert    |
      | UpdatedAt      | generated |

@allure.description:Ensures_creating_a_customer_with_duplicate_company_tax_ids_returns_a_conflict_response.
Feature: Create customer duplicate companies conflict

  Scenario: Create customer with duplicate company tax ids returns conflict response
    Given I have a create customer duplicate companies request payload
      | Field                                      | Value                                 |
      | ExternalId                                 | auth-customer-duplicate-companies-123 |
      | Individual.FirstName                       | Jane                                  |
      | Individual.LastName                        | CompanyDuplicate                      |
      | Individual.Email                           | jane.companyduplicate@example.com     |
      | Individual.Phone                           | 987654321                             |
      | Individual.BillingAddress.PostalCode       | 01-001                                |
      | Individual.BillingAddress.City             | Warsaw                                |
      | Individual.BillingAddress.Street           | Conflict Street                       |
      | Individual.BillingAddress.BuildingNumber   | 11                                    |
      | Individual.BillingAddress.ApartmentNumber  | 21                                    |
      | Individual.ShippingAddress.PostalCode      | 01-002                                |
      | Individual.ShippingAddress.City            | Krakow                                |
      | Individual.ShippingAddress.Street          | Duplicate Avenue                      |
      | Individual.ShippingAddress.BuildingNumber  | 16                                    |
      | Individual.ShippingAddress.ApartmentNumber | 26                                    |
    And the duplicate companies are included in the create customer payload
      | CompanyName    | TaxId      | BillingAddress.PostalCode | BillingAddress.City | BillingAddress.Street | BillingAddress.BuildingNumber | BillingAddress.ApartmentNumber | ShippingAddress.PostalCode | ShippingAddress.City | ShippingAddress.Street | ShippingAddress.BuildingNumber | ShippingAddress.ApartmentNumber |
      | First Company  | 1234567890 | 30-300                    | Poznan              | Business              | 3                             | 15                             | 40-400                     | Lodz                 | Industry               | 4                              | 16                              |
      | Second Company | 1234567890 | 30-300                    | Poznan              | Business              | 3                             | 15                             | 40-400                     | Lodz                 | Industry               | 4                              | 16                              |
    When I submit the create customer duplicate companies request
    Then the create customer duplicate companies request fails with a conflict error
      | Field          | Value                                                                                                  |
      | StatusCode     | 409                                                                                                    |
      | Status         | 409                                                                                                    |
      | Title          | Conflict.                                                                                              |
      | Type           | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8                                           |
      | Instance       | /customers                                                                                             |
      | DetailContains | Resource CompanyData identified by id 1234567890 already exists in db. Error in action CreateCustomer. |
      | HasTraceId     | true                                                                                                   |

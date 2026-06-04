@allure.description:Ensures_creating_an_individual_customer_and_adding_company_data_return_success_responses_with_customer_payload_fields.
Feature: Create customer with company data

  Scenario: Create individual customer and add company data returns customer data
    Given I have a valid create customer request
      | Field                                | Value                    |
      | ExternalId                           | auth-customer-678901     |
      | Individual.FirstName                 | John                     |
      | Individual.LastName                  | Customer                 |
      | Individual.Email                     | john.customer@db.com     |
      | Individual.Phone                     | 123456789                |
      | Individual.BillingAddress.PostalCode | 00-001                   |
      | Individual.BillingAddress.City       | Warsaw                   |
      | Individual.BillingAddress.Street     | Billing Street           |
      | Individual.BillingAddress.BuildingNumber | 10                  |
      | Individual.BillingAddress.ApartmentNumber | 12                  |
      | Individual.ShippingAddress.PostalCode | 30-002                  |
      | Individual.ShippingAddress.City      | Krakow                   |
      | Individual.ShippingAddress.Street    | Shipping Avenue          |
      | Individual.ShippingAddress.BuildingNumber | 20                 |
      | Individual.ShippingAddress.ApartmentNumber |                     |
    When I submit the create customer request
    Then the customer profile is created successfully
      | Field                                | Value                    |
      | StatusCode                           | 200                      |
      | HasId                                | true                     |
      | ExternalId                           | auth-customer-678901     |
      | Individual.FirstName                 | John                     |
      | Individual.LastName                  | Customer                 |
      | Individual.Email                     | john.customer@db.com     |
      | Individual.Phone                     | 123456789                |
      | Individual.BillingAddress.PostalCode | 00-001                   |
      | Individual.BillingAddress.City       | Warsaw                   |
      | Individual.BillingAddress.Street     | Billing Street           |
      | Individual.BillingAddress.BuildingNumber | 10                  |
      | Individual.BillingAddress.ApartmentNumber | 12                  |
      | Individual.ShippingAddress.PostalCode | 30-002                  |
      | Individual.ShippingAddress.City      | Krakow                   |
      | Individual.ShippingAddress.Street    | Shipping Avenue          |
      | Individual.ShippingAddress.BuildingNumber | 20                 |
      | Individual.ShippingAddress.ApartmentNumber |                     |
      | Companies.Count                      | 0                        |
    And I have valid company data for the customer
      | Field                         | Value                 |
      | TaxId                         | 1234567890            |
      | CompanyName                   | Customer Company Ltd  |
      | BillingAddress.PostalCode     | 00-003                |
      | BillingAddress.City           | Warsaw                |
      | BillingAddress.Street         | Company Billing       |
      | BillingAddress.BuildingNumber | 30                    |
      | BillingAddress.ApartmentNumber | 3                    |
      | ShippingAddress.PostalCode    | 50-004                |
      | ShippingAddress.City          | Wroclaw               |
      | ShippingAddress.Street        | Company Shipping      |
      | ShippingAddress.BuildingNumber | 40                   |
      | ShippingAddress.ApartmentNumber |                     |
    When I submit the add company request
    Then the customer profile contains the new company data
      | Field                          | Value                 |
      | StatusCode                     | 200                   |
      | HasId                          | true                  |
      | ExternalId                     | auth-customer-678901  |
      | Individual.FirstName           | John                  |
      | Individual.LastName            | Customer              |
      | Companies.Count                | 1                     |
      | Company.HasId                  | true                  |
      | Company.TaxId                  | 1234567890            |
      | Company.CompanyName            | Customer Company Ltd  |
      | Company.BillingAddress.PostalCode | 00-003             |
      | Company.BillingAddress.City    | Warsaw                |
      | Company.BillingAddress.Street  | Company Billing       |
      | Company.BillingAddress.BuildingNumber | 30              |
      | Company.BillingAddress.ApartmentNumber | 3               |
      | Company.ShippingAddress.PostalCode | 50-004            |
      | Company.ShippingAddress.City   | Wroclaw               |
      | Company.ShippingAddress.Street | Company Shipping      |
      | Company.ShippingAddress.BuildingNumber | 40             |
      | Company.ShippingAddress.ApartmentNumber |                 |

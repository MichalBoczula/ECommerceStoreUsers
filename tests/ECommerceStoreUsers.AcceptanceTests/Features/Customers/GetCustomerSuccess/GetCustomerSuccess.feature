@allure.description:Ensures_getting_an_existing_customer_profile_returns_a_success_response_with_the_customer_payload_fields.
Feature: Get customer profile

  Scenario: Get customer profile by external id returns existing customer data
    Given a customer profile exists for get customer request
      | Field                                      | Value                    |
      | ExternalId                                 | auth-get-customer-123456 |
      | Individual.FirstName                       | Get                      |
      | Individual.LastName                        | Customer                 |
      | Individual.Email                           | get.customer@db.com      |
      | Individual.Phone                           | 123456789                |
      | Individual.BillingAddress.PostalCode       | 00-001                   |
      | Individual.BillingAddress.City             | Warsaw                   |
      | Individual.BillingAddress.Street           | Main Street              |
      | Individual.BillingAddress.BuildingNumber   | 10                       |
      | Individual.BillingAddress.ApartmentNumber  | 20                       |
      | Individual.ShippingAddress.PostalCode      | 00-002                   |
      | Individual.ShippingAddress.City            | Krakow                   |
      | Individual.ShippingAddress.Street          | Shipping Street          |
      | Individual.ShippingAddress.BuildingNumber  | 15                       |
      | Individual.ShippingAddress.ApartmentNumber | 25                       |
    When I request the customer profile by external id
      | Field      | Value                    |
      | ExternalId | auth-get-customer-123456 |
    Then the customer profile is returned successfully
      | Field                                      | Value                    |
      | StatusCode                                 | 200                      |
      | HasId                                      | true                     |
      | ExternalId                                 | auth-get-customer-123456 |
      | CompaniesCount                             | 0                        |
      | HasUpdatedAt                               | true                     |
      | Individual.FirstName                       | Get                      |
      | Individual.LastName                        | Customer                 |
      | Individual.Email                           | get.customer@db.com      |
      | Individual.Phone                           | 123456789                |
      | Individual.BillingAddress.PostalCode       | 00-001                   |
      | Individual.BillingAddress.City             | Warsaw                   |
      | Individual.BillingAddress.Street           | Main Street              |
      | Individual.BillingAddress.BuildingNumber   | 10                       |
      | Individual.BillingAddress.ApartmentNumber  | 20                       |
      | Individual.ShippingAddress.PostalCode      | 00-002                   |
      | Individual.ShippingAddress.City            | Krakow                   |
      | Individual.ShippingAddress.Street          | Shipping Street          |
      | Individual.ShippingAddress.BuildingNumber  | 15                       |
      | Individual.ShippingAddress.ApartmentNumber | 25                       |

@allure.description:Ensures_creating_an_individual_customer_profile_returns_a_success_response_with_the_created_payload_fields.
Feature: Create customer profile

  Scenario: Create individual customer profile returns created customer data
    Given I have a valid create individual customer request
      | Field                                      | Value                 |
      | ExternalId                                 | auth-customer-123456  |
      | Individual.FirstName                       | John                  |
      | Individual.LastName                        | Doe                   |
      | Individual.Email                           | john.doe@example.com  |
      | Individual.Phone                           | 123456789             |
      | Individual.BillingAddress.PostalCode       | 00-001                |
      | Individual.BillingAddress.City             | Warsaw                |
      | Individual.BillingAddress.Street           | Main Street           |
      | Individual.BillingAddress.BuildingNumber   | 10                    |
      | Individual.BillingAddress.ApartmentNumber  | 20                    |
      | Individual.ShippingAddress.PostalCode      | 00-002                |
      | Individual.ShippingAddress.City            | Krakow                |
      | Individual.ShippingAddress.Street          | Shipping Street       |
      | Individual.ShippingAddress.BuildingNumber  | 15                    |
      | Individual.ShippingAddress.ApartmentNumber | 25                    |
    When I submit the create individual customer request
    Then the individual customer profile is created successfully
      | Field                                      | Value                 |
      | StatusCode                                 | 200                   |
      | HasId                                      | true                  |
      | ExternalId                                 | auth-customer-123456  |
      | CompaniesCount                             | 0                     |
      | HasUpdatedAt                               | true                  |
      | Individual.FirstName                       | John                  |
      | Individual.LastName                        | Doe                   |
      | Individual.Email                           | john.doe@example.com  |
      | Individual.Phone                           | 123456789             |
      | Individual.BillingAddress.PostalCode       | 00-001                |
      | Individual.BillingAddress.City             | Warsaw                |
      | Individual.BillingAddress.Street           | Main Street           |
      | Individual.BillingAddress.BuildingNumber   | 10                    |
      | Individual.BillingAddress.ApartmentNumber  | 20                    |
      | Individual.ShippingAddress.PostalCode      | 00-002                |
      | Individual.ShippingAddress.City            | Krakow                |
      | Individual.ShippingAddress.Street          | Shipping Street       |
      | Individual.ShippingAddress.BuildingNumber  | 15                    |
      | Individual.ShippingAddress.ApartmentNumber | 25                    |

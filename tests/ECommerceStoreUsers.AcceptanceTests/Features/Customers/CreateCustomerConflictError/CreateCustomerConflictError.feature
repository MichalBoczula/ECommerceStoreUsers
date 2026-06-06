@allure.description:Ensures_creating_a_duplicate_customer_profile_returns_a_conflict_response.
Feature: Create customer profile conflict

  Scenario: Create duplicate individual customer profile returns conflict response
    Given I have a valid create individual customer request payload
      | Field                                      | Value                          |
      | ExternalId                                 | auth-customer-conflict-123456  |
      | Individual.FirstName                       | Jane                           |
      | Individual.LastName                        | Duplicate                      |
      | Individual.Email                           | jane.duplicate@example.com     |
      | Individual.Phone                           | 987654321                      |
      | Individual.BillingAddress.PostalCode       | 01-001                         |
      | Individual.BillingAddress.City             | Warsaw                         |
      | Individual.BillingAddress.Street           | Conflict Street                |
      | Individual.BillingAddress.BuildingNumber   | 11                             |
      | Individual.BillingAddress.ApartmentNumber  | 21                             |
      | Individual.ShippingAddress.PostalCode      | 01-002                         |
      | Individual.ShippingAddress.City            | Krakow                         |
      | Individual.ShippingAddress.Street          | Duplicate Avenue               |
      | Individual.ShippingAddress.BuildingNumber  | 16                             |
      | Individual.ShippingAddress.ApartmentNumber | 26                             |
    When I submit the same create customer request twice
    Then the customer profile creation fails with a conflict error
      | Field          | Value                                                                                                    |
      | StatusCode     | 409                                                                                                      |
      | Status         | 409                                                                                                      |
      | Title          | Conflict.                                                                                                |
      | Type           | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8                                             |
      | Instance       | /customers                                                                                               |
      | DetailContains | Resource Customer identified by id auth-customer-conflict-123456 already exists in db. Error in action CreateCustomer. |
      | HasTraceId     | true                                                                                                     |

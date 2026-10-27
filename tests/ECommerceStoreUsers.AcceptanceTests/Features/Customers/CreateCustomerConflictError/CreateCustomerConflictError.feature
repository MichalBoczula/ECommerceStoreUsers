@allure.description:Ensures_creating_a_duplicate_customer_profile_returns_a_conflict_response.
Feature: Create customer profile conflict

  @allure.label.scenarioId:REF07_CreateCustomer_01
  Scenario: Create duplicate individual customer profile returns conflict response
    A customer profile already exists for the supplied external identity.
    A second create request returns a conflict problem instead of creating a duplicate profile.
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
    And the original Customer exists before repeating its request
    And I record all Users data before this business request
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
    And the complete business conflict detail is "Resource Customer identified by id auth-customer-conflict-123456 already exists in db. Error in action CreateCustomer."
    And every Users collection is unchanged after business rejection

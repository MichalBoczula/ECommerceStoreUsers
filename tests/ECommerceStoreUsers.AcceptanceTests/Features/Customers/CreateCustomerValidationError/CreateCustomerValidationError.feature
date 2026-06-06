@allure.description:Ensures_creating_an_individual_customer_profile_with_an_invalid_address_returns_a_bad_request_with_validation_errors.
Feature: Create customer profile validation

  Scenario: Create individual customer profile with invalid billing address returns bad request
    Given I have an invalid individual create customer request
      | Field                                      | Value                |
      | ExternalId                                 | auth-customer-654321 |
      | Individual.FirstName                       | John                 |
      | Individual.LastName                        | Doe                  |
      | Individual.Email                           | john.doe@example.com |
      | Individual.Phone                           | 123456789            |
      | Individual.BillingAddress.PostalCode       | 00-001               |
      | Individual.BillingAddress.City             |                      |
      | Individual.BillingAddress.Street           | Main Street          |
      | Individual.BillingAddress.BuildingNumber   | 10                   |
      | Individual.BillingAddress.ApartmentNumber  | 20                   |
      | Individual.ShippingAddress.PostalCode      | 00-002               |
      | Individual.ShippingAddress.City            | Krakow               |
      | Individual.ShippingAddress.Street          | Shipping Street      |
      | Individual.ShippingAddress.BuildingNumber  | 15                   |
      | Individual.ShippingAddress.ApartmentNumber | 25                   |
    When I submit the create customer request for validation
    Then the customer profile creation fails with a validation error
      | Field      | Value                                          |
      | StatusCode | 400                                            |
      | Message    | City cannot be empty or white space.           |
      | Name       | AddressCityValidationRule                      |
      | Entity     | Address                                        |

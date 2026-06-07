@allure.description:Ensures_updating_an_existing_customer_individual_data_returns_a_success_response_with_the_updated_individual_payload_fields.
Feature: Update customer individual data

  Scenario: Update customer individual data returns updated customer profile
    Given a customer exists for update individual request
      | Field                                      | Value                         |
      | ExternalId                                 | customer-update-individual-123456 |
      | Individual.FirstName                       | Alice                         |
      | Individual.LastName                        | Client                        |
      | Individual.Email                           | alice.client@example.com      |
      | Individual.Phone                           | 123456789                     |
      | Individual.BillingAddress.PostalCode       | 00-001                        |
      | Individual.BillingAddress.City             | Warsaw                        |
      | Individual.BillingAddress.Street           | Main Street                   |
      | Individual.BillingAddress.BuildingNumber   | 10                            |
      | Individual.BillingAddress.ApartmentNumber  | 20                            |
      | Individual.ShippingAddress.PostalCode      | 00-002                        |
      | Individual.ShippingAddress.City            | Krakow                        |
      | Individual.ShippingAddress.Street          | Shipping Street               |
      | Individual.ShippingAddress.BuildingNumber  | 15                            |
      | Individual.ShippingAddress.ApartmentNumber | 25                            |
    And I have a valid update individual request
      | Field                                      | Value                         |
      | Individual.FirstName                       | Alice                         |
      | Individual.LastName                        | Client                        |
      | Individual.Email                           | alice.updated@example.com     |
      | Individual.Phone                           | 987654321                     |
      | Individual.BillingAddress.PostalCode       | 00-950                        |
      | Individual.BillingAddress.City             | Warsaw                        |
      | Individual.BillingAddress.Street           | Prosta                        |
      | Individual.BillingAddress.BuildingNumber   | 20                            |
      | Individual.BillingAddress.ApartmentNumber  | 15                            |
      | Individual.ShippingAddress.PostalCode      | 80-001                        |
      | Individual.ShippingAddress.City            | Gdansk                        |
      | Individual.ShippingAddress.Street          | Portowa                       |
      | Individual.ShippingAddress.BuildingNumber  | 4A                            |
      | Individual.ShippingAddress.ApartmentNumber | 2                             |
    When I submit the update individual request
    Then the individual data is updated successfully
      | Field                                      | Value                         |
      | StatusCode                                 | 200                           |
      | HasCustomerId                              | true                          |
      | ExternalId                                 | customer-update-individual-123456 |
      | CompanyCount                               | 0                             |
      | Individual.FirstName                       | Alice                         |
      | Individual.LastName                        | Client                        |
      | Individual.Email                           | alice.updated@example.com     |
      | Individual.Phone                           | 987654321                     |
      | Individual.BillingAddress.PostalCode       | 00-950                        |
      | Individual.BillingAddress.City             | Warsaw                        |
      | Individual.BillingAddress.Street           | Prosta                        |
      | Individual.BillingAddress.BuildingNumber   | 20                            |
      | Individual.BillingAddress.ApartmentNumber  | 15                            |
      | Individual.ShippingAddress.PostalCode      | 80-001                        |
      | Individual.ShippingAddress.City            | Gdansk                        |
      | Individual.ShippingAddress.Street          | Portowa                       |
      | Individual.ShippingAddress.BuildingNumber  | 4A                            |
      | Individual.ShippingAddress.ApartmentNumber | 2                             |

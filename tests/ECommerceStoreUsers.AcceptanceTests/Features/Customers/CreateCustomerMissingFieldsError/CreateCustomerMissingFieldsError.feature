@allure.description:Ensures_creating_a_customer_with_a_payload_missing_required_individual_json_properties_returns_a_bad_request.
Feature: Create customer profile deserialization validation

  Scenario: Create customer profile with missing individual email and phone returns deserialization error
    Given I have a create customer request payload missing the individual email and phone
      | Field                                      | Value                         |
      | ExternalId                                 | auth-customer-missing-123456  |
      | Individual.FirstName                       | John                          |
      | Individual.LastName                        | Doe                           |
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
    When I submit the incomplete create customer request
    Then the create customer response indicates a json deserialization failure
      | Field           | Value                                                                         |
      | StatusCode      | 400                                                                           |
      | Title           | Invalid JSON payload.                                                         |
      | Detail          | JSON payload for IndividualDataRequestDto is missing required properties: email, phone. |
      | Type            | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1                   |
      | MissingProperty | email                                                                         |
      | MissingProperty | phone                                                                         |

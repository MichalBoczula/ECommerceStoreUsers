@allure.description:Ensures_updating_customer_individual_data_with_a_payload_missing_required_individual_json_properties_returns_a_bad_request.
Feature: Update customer individual data deserialization validation

  Scenario: Update customer individual data with missing individual email and phone returns deserialization error
    Given a customer exists for update individual missing fields request
      | Field                   | Value                                  |
      | ExternalId              | customer-update-individual-missing-123 |
      | FirstName               | Existing                               |
      | LastName                | Customer                               |
      | Email                   | existing.customer@db.com               |
      | Phone                   | 111222333                              |
      | BillingPostalCode       | 00-001                                 |
      | BillingCity             | Warsaw                                 |
      | BillingStreet           | Main Street                            |
      | BillingBuildingNumber   | 10                                     |
      | BillingApartmentNumber  | 20                                     |
      | ShippingPostalCode      | 00-002                                 |
      | ShippingCity            | Krakow                                 |
      | ShippingStreet          | Shipping Street                        |
      | ShippingBuildingNumber  | 15                                     |
      | ShippingApartmentNumber | 25                                     |
    And I have an update individual request payload missing the individual email and phone
      | Field                            | Value           |
      | Individual.FirstName             | Updated         |
      | Individual.LastName              | Customer        |
      | Individual.BillingPostalCode     | 10-100          |
      | Individual.BillingCity           | Poznan          |
      | Individual.BillingStreet         | Update Street   |
      | Individual.BillingBuildingNumber | 5               |
      | Individual.BillingApartmentNumber | 11              |
      | Individual.ShippingPostalCode    | 20-200          |
      | Individual.ShippingCity          | Wroclaw         |
      | Individual.ShippingStreet        | Delivery Street |
      | Individual.ShippingBuildingNumber | 7B              |
      | Individual.ShippingApartmentNumber | 14             |
    When I submit the incomplete update individual request
    Then the update individual response indicates a json deserialization failure
      | Field           | Value                                                                         |
      | StatusCode      | 400                                                                           |
      | Title           | Invalid JSON payload.                                                         |
      | Detail          | JSON payload for IndividualDataRequestDto is missing required properties: email, phone. |
      | Type            | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1                   |
      | MissingProperty | email                                                                         |
      | MissingProperty | phone                                                                         |

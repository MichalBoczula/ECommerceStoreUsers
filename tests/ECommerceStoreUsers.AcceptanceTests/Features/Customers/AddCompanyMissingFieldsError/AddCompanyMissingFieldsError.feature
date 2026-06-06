@allure.description:Ensures_adding_a_company_with_a_payload_missing_required_json_properties_returns_a_bad_request_after_customer_creation.
Feature: Add company deserialization validation

  Scenario: Add company to existing customer with missing company name returns deserialization error
    Given a customer exists for add company missing fields request
      | Field                   | Value                                |
      | ExternalId              | customer-company-missing-123456      |
      | FirstName               | Missing                              |
      | LastName                | Company                              |
      | Email                   | missing.company.customer@db.com      |
      | Phone                   | 123456789                            |
      | BillingPostalCode       | 00-001                               |
      | BillingCity             | Warsaw                               |
      | BillingStreet           | Marszalkowska                        |
      | BillingBuildingNumber   | 10                                   |
      | BillingApartmentNumber  | 5                                    |
      | ShippingPostalCode      | 30-002                               |
      | ShippingCity            | Krakow                               |
      | ShippingStreet          | Dluga                                |
      | ShippingBuildingNumber  | 12                                   |
      | ShippingApartmentNumber | 7                                    |
    And I have an add company request payload missing the company name
      | Field                   | Value                                |
      | TaxId                   | 1234567890                           |
      | BillingPostalCode       | 00-950                               |
      | BillingCity             | Warsaw                               |
      | BillingStreet           | Prosta                               |
      | BillingBuildingNumber   | 20                                   |
      | BillingApartmentNumber  | 15                                   |
      | ShippingPostalCode      | 80-001                               |
      | ShippingCity            | Gdansk                               |
      | ShippingStreet          | Portowa                              |
      | ShippingBuildingNumber  | 4A                                   |
      | ShippingApartmentNumber | 2                                    |
    When I submit the incomplete add company request
    Then the add company response indicates a json deserialization failure
      | Field           | Value                                                                                 |
      | StatusCode      | 400                                                                                   |
      | Title           | Invalid JSON payload.                                                                 |
      | Detail          | JSON payload for AddCompanyRequestDto is missing required properties: companyName.     |
      | Type            | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1                           |
      | MissingProperty | companyName                                                                           |

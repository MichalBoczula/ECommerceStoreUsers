@allure.description:Ensures_updating_customer_company_data_with_a_payload_missing_required_update_company_json_properties_returns_a_bad_request.
Feature: Update customer company data deserialization validation

  Scenario: Update customer company data with missing company name and shipping address returns deserialization error
    Given a customer with a company exists for update company missing fields request
      | Field                           | Value                               |
      | ExternalId                      | customer-update-company-missing-123 |
      | FirstName                       | Existing                            |
      | LastName                        | CompanyOwner                        |
      | Email                           | update.company.owner@db.com         |
      | Phone                           | 111222333                           |
      | BillingPostalCode               | 00-001                              |
      | BillingCity                     | Warsaw                              |
      | BillingStreet                   | Main Street                         |
      | BillingBuildingNumber           | 10                                  |
      | BillingApartmentNumber          | 20                                  |
      | ShippingPostalCode              | 00-002                              |
      | ShippingCity                    | Krakow                              |
      | ShippingStreet                  | Shipping Street                     |
      | ShippingBuildingNumber          | 15                                  |
      | ShippingApartmentNumber         | 25                                  |
      | CompanyTaxId                    | 1234567890                          |
      | CompanyName                     | Existing Company Sp. z o.o.         |
      | CompanyBillingPostalCode        | 10-100                              |
      | CompanyBillingCity              | Poznan                              |
      | CompanyBillingStreet            | Company Billing Street              |
      | CompanyBillingBuildingNumber    | 5                                   |
      | CompanyBillingApartmentNumber   | 11                                  |
      | CompanyShippingPostalCode       | 20-200                              |
      | CompanyShippingCity             | Wroclaw                             |
      | CompanyShippingStreet           | Company Shipping Street             |
      | CompanyShippingBuildingNumber   | 7B                                  |
      | CompanyShippingApartmentNumber  | 14                                  |
    And I have an update company request payload missing the company name and shipping address
      | Field                           | Value                  |
      | TaxId                           | 0987654321             |
      | BillingPostalCode               | 30-300                 |
      | BillingCity                     | Gdansk                 |
      | BillingStreet                   | Updated Billing Street |
      | BillingBuildingNumber           | 22                     |
      | BillingApartmentNumber          | 3                      |
    When I submit the incomplete update company request
    Then the update company response indicates a json deserialization failure
      | Field           | Value                                                                                     |
      | StatusCode      | 400                                                                                       |
      | Title           | Invalid JSON payload.                                                                     |
      | Detail          | JSON payload for UpdateCompanyRequestDto is missing required properties: companyName, shippingAddress. |
      | Type            | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1                               |
      | MissingProperty | companyName                                                                               |
      | MissingProperty | shippingAddress                                                                           |

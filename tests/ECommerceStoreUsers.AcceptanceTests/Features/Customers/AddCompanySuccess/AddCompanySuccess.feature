@allure.description:Ensures_adding_a_company_to_an_existing_customer_returns_a_success_response_with_the_company_payload_fields.
Feature: Add customer company

  Scenario: Add company to customer returns updated customer data
    Given a customer exists for add company request
      | Field                           | Value                         |
      | ExternalId                      | customer-company-123456       |
      | FirstName                       | Company                       |
      | LastName                        | Client                        |
      | Email                           | company.client@db.com         |
      | Phone                           | 123456789                     |
      | BillingPostalCode               | 00-001                        |
      | BillingCity                     | Warsaw                        |
      | BillingStreet                   | Marszalkowska                 |
      | BillingBuildingNumber           | 10                            |
      | BillingApartmentNumber          | 5                             |
      | ShippingPostalCode              | 30-002                        |
      | ShippingCity                    | Krakow                        |
      | ShippingStreet                  | Dluga                         |
      | ShippingBuildingNumber          | 12                            |
      | ShippingApartmentNumber         | 7                             |
    And I have a valid add company request
      | Field                           | Value                         |
      | TaxId                           | 1234567890                    |
      | CompanyName                     | Example Company Sp. z o.o.    |
      | BillingPostalCode               | 00-950                        |
      | BillingCity                     | Warsaw                        |
      | BillingStreet                   | Prosta                        |
      | BillingBuildingNumber           | 20                            |
      | BillingApartmentNumber          | 15                            |
      | ShippingPostalCode              | 80-001                        |
      | ShippingCity                    | Gdansk                        |
      | ShippingStreet                  | Portowa                       |
      | ShippingBuildingNumber          | 4A                            |
      | ShippingApartmentNumber         | 2                             |
    When I submit the add company request
    Then the company is added to the customer successfully
      | Field                           | Value                         |
      | StatusCode                      | 200                           |
      | HasCustomerId                   | true                          |
      | ExternalId                      | customer-company-123456       |
      | CompanyCount                    | 1                             |
      | HasCompanyId                    | true                          |
      | TaxId                           | 1234567890                    |
      | CompanyName                     | Example Company Sp. z o.o.    |
      | BillingPostalCode               | 00-950                        |
      | BillingCity                     | Warsaw                        |
      | BillingStreet                   | Prosta                        |
      | BillingBuildingNumber           | 20                            |
      | BillingApartmentNumber          | 15                            |
      | ShippingPostalCode              | 80-001                        |
      | ShippingCity                    | Gdansk                        |
      | ShippingStreet                  | Portowa                       |
      | ShippingBuildingNumber          | 4A                            |
      | ShippingApartmentNumber         | 2                             |

@allure.description:Ensures_updating_a_company_that_does_not_exist_for_an_existing_customer_returns_a_not_found_problem_details_response.
Feature: Update company not found

  Scenario: Update missing company for existing customer returns not found response
    Given a customer with a company exists for update company not found request
      | Field                                      | Value                                      |
      | ExternalId                                 | auth-customer-update-company-not-found-123 |
      | Individual.FirstName                       | Update                                     |
      | Individual.LastName                        | CompanyNotFound                           |
      | Individual.Email                           | update.company.notfound@example.com        |
      | Individual.Phone                           | 111222333                                  |
      | Individual.BillingAddress.PostalCode       | 00-950                                     |
      | Individual.BillingAddress.City             | Warsaw                                     |
      | Individual.BillingAddress.Street           | Existing Customer Street                   |
      | Individual.BillingAddress.BuildingNumber   | 10                                         |
      | Individual.BillingAddress.ApartmentNumber  | 5                                          |
      | Individual.ShippingAddress.PostalCode      | 80-001                                     |
      | Individual.ShippingAddress.City            | Gdansk                                     |
      | Individual.ShippingAddress.Street          | Existing Customer Shipping                 |
      | Individual.ShippingAddress.BuildingNumber  | 12                                         |
      | Individual.ShippingAddress.ApartmentNumber | 7                                          |
      | ExistingCompany.TaxId                       | 1234509876                                 |
      | ExistingCompany.CompanyName                 | Existing Company Sp. z o.o.                |
      | ExistingCompany.BillingAddress.PostalCode  | 30-300                                     |
      | ExistingCompany.BillingAddress.City        | Poznan                                     |
      | ExistingCompany.BillingAddress.Street      | Business                                   |
      | ExistingCompany.BillingAddress.BuildingNumber | 3                                      |
      | ExistingCompany.BillingAddress.ApartmentNumber | 15                                     |
      | ExistingCompany.ShippingAddress.PostalCode | 40-400                                     |
      | ExistingCompany.ShippingAddress.City       | Lodz                                       |
      | ExistingCompany.ShippingAddress.Street     | Industry                                   |
      | ExistingCompany.ShippingAddress.BuildingNumber | 4                                     |
      | ExistingCompany.ShippingAddress.ApartmentNumber | 16                                    |
    And I have an update missing company request
      | Field                                      | Value                                |
      | CompanyId                                  | 44444444-4444-4444-4444-444444444444 |
      | TaxId                                      | 9988776655                           |
      | CompanyName                                | Missing Company Updated Sp. z o.o.   |
      | BillingAddress.PostalCode                  | 50-500                               |
      | BillingAddress.City                        | Wroclaw                              |
      | BillingAddress.Street                      | Update Billing                       |
      | BillingAddress.BuildingNumber              | 50                                   |
      | BillingAddress.ApartmentNumber             | 55                                   |
      | ShippingAddress.PostalCode                 | 60-600                               |
      | ShippingAddress.City                       | Szczecin                             |
      | ShippingAddress.Street                     | Update Shipping                      |
      | ShippingAddress.BuildingNumber             | 60                                   |
      | ShippingAddress.ApartmentNumber            | 66                                   |
    When I submit the update missing company request
    Then the update company request fails with a not found response
      | Field            | Value                                                                                                                                        |
      | StatusCode       | 404                                                                                                                                          |
      | Title            | Resource not found.                                                                                                                          |
      | Detail           | Resource CompanyData identified by id {CompanyId} cannot be found in database during action LoadCompany.                                     |
      | Type             | https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4                                                                                  |
      | InstanceTemplate | /customers/{CustomerId}/companies/{CompanyId}                                                                                                |
      | HasTraceId       | true                                                                                                                                         |

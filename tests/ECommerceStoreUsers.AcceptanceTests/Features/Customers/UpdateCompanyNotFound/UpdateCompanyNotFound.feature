@allure.description:Ensures_updating_a_company_that_does_not_exist_for_an_existing_customer_returns_a_not_found_problem_details_response.
Feature: Update company not found

  @allure.label.scenarioId:REF07_UpdateCompany_03
  Scenario: Update missing company for existing customer returns not found response
    A company-update request identifies an existing customer but an absent company.
    The API returns a not-found problem for the missing company.
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

  @allure.label.scenarioId:REF11_UpdateCompany_MissingCustomer
  Scenario: Updating a company for an absent Customer preserves all stored profiles
    A valid company replacement identifies an absent Customer UUID rather than an absent company inside an existing Customer.
    The initial lookup returns the complete safe Customer 404 problem and all five populated collections remain unchanged.
    Given representative Users data exists for framework rejection checks
    And I record all Users data before this framework request
    And the framework request is
      | Field       | Value                                                                                           |
      | Method      | PUT                                                                                             |
      | Path        | /customers/99999999-9999-9999-9999-999999999999/companies/88888888-8888-8888-8888-888888888888 |
      | ContentType | application/json; charset=utf-8                                                                 |
      | Body        | <docstring>                                                                                     |
    And the exact framework body is
      """
      {
        "taxId":"1234567890","companyName":"Missing Customer Company",
        "billingAddress":{"postalCode":"00-001","city":"Warsaw","street":"Example Street","buildingNumber":"10","apartmentNumber":null},
        "shippingAddress":{"postalCode":"00-002","city":"Krakow","street":"Shipping Street","buildingNumber":"20","apartmentNumber":null}
      }
      """
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                                                                                                                                             |
      | Status            | 404                                                                                                                                               |
      | Code              | resource_not_found                                                                                                                                |
      | Title             | Resource not found.                                                                                                                               |
      | Detail            | Resource Customer identified by id 99999999-9999-9999-9999-999999999999 cannot be found in database during action UpdateCompany.                  |
      | MissingProperties | []                                                                                                                                                |
      | Allow             | []                                                                                                                                                |
    And every Users collection is unchanged

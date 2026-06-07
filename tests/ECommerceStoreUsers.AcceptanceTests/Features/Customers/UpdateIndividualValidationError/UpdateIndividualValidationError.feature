@allure.description:Ensures_updating_existing_customer_individual_data_with_invalid_contact_values_returns_a_bad_request_with_validation_errors.
Feature: Update customer individual data validation

  Scenario: Update existing customer individual data with invalid contact values returns bad request
    Given a customer exists for update individual validation request
      | Field                           | Value                                 |
      | ExternalId                      | customer-update-individual-validation |
      | FirstName                       | Update                                |
      | LastName                        | Individual                            |
      | Email                           | update.individual@db.com              |
      | Phone                           | 123456789                             |
      | BillingPostalCode               | 00-001                                |
      | BillingCity                     | Warsaw                                |
      | BillingStreet                   | Marszalkowska                         |
      | BillingBuildingNumber           | 10                                    |
      | BillingApartmentNumber          | 5                                     |
      | ShippingPostalCode              | 30-002                                |
      | ShippingCity                    | Krakow                                |
      | ShippingStreet                  | Dluga                                 |
      | ShippingBuildingNumber          | 12                                    |
      | ShippingApartmentNumber         | 7                                     |
    And I have an invalid update individual request
      | Field                           | Value                                 |
      | FirstName                       | Updated                               |
      | LastName                        | Customer                              |
      | Email                           | invalid-email                         |
      | Phone                           | 12345ab                               |
      | BillingPostalCode               | 00-950                                |
      | BillingCity                     | Warsaw                                |
      | BillingStreet                   | Prosta                                |
      | BillingBuildingNumber           | 20                                    |
      | BillingApartmentNumber          | 15                                    |
      | ShippingPostalCode              | 80-001                                |
      | ShippingCity                    | Gdansk                                |
      | ShippingStreet                  | Portowa                               |
      | ShippingBuildingNumber          | 4A                                    |
      | ShippingApartmentNumber         | 2                                     |
    When I submit the update individual request for validation
    Then updating the individual data fails with validation errors
      | Field                           | Value                                                                    |
      | StatusCode                      | 400                                                                      |
      | EmailMessage                    | Email must be a valid format (address@domain.something).                 |
      | EmailName                       | IndividualDataEmailValidationRule                                        |
      | EmailEntity                     | IndividualData                                                           |
      | PhoneMessage                    | Phone must contain only digits and be between 7 and 10 characters long.  |
      | PhoneName                       | IndividualDataPhoneValidationRule                                        |
      | PhoneEntity                     | IndividualData                                                           |

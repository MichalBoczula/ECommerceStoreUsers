Feature: Active business validation and data preservation
  Complete policy errors and domain conflicts describe the existing business rules.
  Invalid requests preserve every Customer, Admin, history and Favorite document.

  Background:
    Given representative Users data exists for business validation checks
    And the valid "Customer" business payload is
      """
      {
        "externalId": "bdd06-new-customer",
        "individual": {
          "firstName": "Jane",
          "lastName": "Validation",
          "email": "jane.validation@example.com",
          "phone": "123456789",
          "billingAddress": {
            "postalCode": "00-001",
            "city": "Warsaw",
            "street": "Validation Street",
            "buildingNumber": "10",
            "apartmentNumber": "1"
          },
          "shippingAddress": {
            "postalCode": "00-001",
            "city": "Warsaw",
            "street": "Validation Street",
            "buildingNumber": "10",
            "apartmentNumber": "1"
          }
        },
        "companies": [
          {
            "taxId": "9876543210",
            "companyName": "Validation Company",
            "billingAddress": {
              "postalCode": "00-001",
              "city": "Warsaw",
              "street": "Validation Street",
              "buildingNumber": "10",
              "apartmentNumber": "1"
            },
            "shippingAddress": {
              "postalCode": "00-001",
              "city": "Warsaw",
              "street": "Validation Street",
              "buildingNumber": "10",
              "apartmentNumber": "1"
            }
          },
          {
            "taxId": "0987654321",
            "companyName": "Second Validation Company",
            "billingAddress": {
              "postalCode": "00-001",
              "city": "Warsaw",
              "street": "Validation Street",
              "buildingNumber": "10",
              "apartmentNumber": "1"
            },
            "shippingAddress": {
              "postalCode": "00-001",
              "city": "Warsaw",
              "street": "Validation Street",
              "buildingNumber": "10",
              "apartmentNumber": "1"
            }
          }
        ]
      }
      """
    And the valid "Admin" business payload is
      """
      {
        "externalId": "bdd06-new-admin",
        "fullName": "Jane Validation",
        "email": "admin.validation@example.com"
      }
      """
    And the valid "Favorite" business payload is
      """
      {
        "productId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
      }
      """

  Scenario Outline: A single changed JSON value is rejected by <operationId>
    All other members come from the explicit valid payload template.
    Changing only the named value produces exactly the stated error, with no change to any of the five seeded collections.
    Given the business request uses "<operationId>" with method "<Method>" and path "<Path>"
    And I change only these JSON values
      | Pointer   | JsonValue   |
      | <Pointer> | <JsonValue> |
    When I submit the business validation request
    Then the complete business validation problem contains exactly these errors
      | Name   | Message   | Entity   |
      | <Name> | <Message> | <Entity> |
    And every Users collection is unchanged after business rejection

    Examples:
      | operationId           | Method | Path                                                    | Case                                       | Pointer                                      | JsonValue                              | Name                                  | Message                                                                         | Entity         | scenarioId                                                                       |
      | CreateCustomer        | POST   | /customers                                              | ExternalBlank                              | /externalId                                  | "   "                                  | CustomerExternalIdValidationRule      | ExternalId cannot be null or white space.                                       | Customer       | REF06_Validation_CreateCustomer_ExternalBlank                                    |
      | CreateCustomer        | POST   | /customers                                              | ExternalEmptyGuid                          | /externalId                                  | "00000000-0000-0000-0000-000000000000" | CustomerExternalIdValidationRule      | ExternalId cannot be an empty guid.                                             | Customer       | REF06_Validation_CreateCustomer_ExternalEmptyGuid                                |
      | CreateCustomer        | POST   | /customers                                              | firstName                                  | /individual/firstName                        | "12345"                                | IndividualDataFirstNameValidationRule | First Name cannot be empty or white space and must contain at least one letter. | IndividualData | REF06_Validation_CreateCustomer_firstName                                        |
      | CreateCustomer        | POST   | /customers                                              | lastName                                   | /individual/lastName                         | "12345"                                | IndividualDataLastNameValidationRule  | Last Name cannot be empty or white space and must contain at least one letter.  | IndividualData | REF06_Validation_CreateCustomer_lastName                                         |
      | CreateCustomer        | POST   | /customers                                              | email                                      | /individual/email                            | "bad-email"                            | IndividualDataEmailValidationRule     | Email must be a valid format (address@domain.something).                        | IndividualData | REF06_Validation_CreateCustomer_email                                            |
      | CreateCustomer        | POST   | /customers                                              | phone                                      | /individual/phone                            | "123a567"                              | IndividualDataPhoneValidationRule     | Phone must contain only digits and be between 7 and 10 characters long.         | IndividualData | REF06_Validation_CreateCustomer_phone                                            |
      | CreateCustomer        | POST   | /customers                                              | Individual_billingAddress_postalCode       | /individual/billingAddress/postalCode        | ""                                     | AddressPostalCodeValidationRule       | Postal Code cannot be empty or white space.                                     | Address        | REF06_Validation_CreateCustomer_Individual_billingAddress_postalCode             |
      | CreateCustomer        | POST   | /customers                                              | Individual_billingAddress_city             | /individual/billingAddress/city              | "   "                                  | AddressCityValidationRule             | City cannot be empty or white space.                                            | Address        | REF06_Validation_CreateCustomer_Individual_billingAddress_city                   |
      | CreateCustomer        | POST   | /customers                                              | Individual_billingAddress_street           | /individual/billingAddress/street            | null                                   | AddressStreetValidationRule           | Street cannot be empty or white space.                                          | Address        | REF06_Validation_CreateCustomer_Individual_billingAddress_street                 |
      | CreateCustomer        | POST   | /customers                                              | Individual_billingAddress_buildingNumber   | /individual/billingAddress/buildingNumber    | ""                                     | AddressBuildingNumberValidationRule   | Building Number cannot be empty or white space.                                 | Address        | REF06_Validation_CreateCustomer_Individual_billingAddress_buildingNumber         |
      | CreateCustomer        | POST   | /customers                                              | Individual_billingAddress_apartmentNumber  | /individual/billingAddress/apartmentNumber   | "   "                                  | AddressApartmentNumberValidationRule  | Apartment Number cannot be empty or white space.                                | Address        | REF06_Validation_CreateCustomer_Individual_billingAddress_apartmentNumber        |
      | CreateCustomer        | POST   | /customers                                              | Individual_shippingAddress_postalCode      | /individual/shippingAddress/postalCode       | ""                                     | AddressPostalCodeValidationRule       | Postal Code cannot be empty or white space.                                     | Address        | REF06_Validation_CreateCustomer_Individual_shippingAddress_postalCode            |
      | CreateCustomer        | POST   | /customers                                              | Individual_shippingAddress_city            | /individual/shippingAddress/city             | "   "                                  | AddressCityValidationRule             | City cannot be empty or white space.                                            | Address        | REF06_Validation_CreateCustomer_Individual_shippingAddress_city                  |
      | CreateCustomer        | POST   | /customers                                              | Individual_shippingAddress_street          | /individual/shippingAddress/street           | null                                   | AddressStreetValidationRule           | Street cannot be empty or white space.                                          | Address        | REF06_Validation_CreateCustomer_Individual_shippingAddress_street                |
      | CreateCustomer        | POST   | /customers                                              | Individual_shippingAddress_buildingNumber  | /individual/shippingAddress/buildingNumber   | ""                                     | AddressBuildingNumberValidationRule   | Building Number cannot be empty or white space.                                 | Address        | REF06_Validation_CreateCustomer_Individual_shippingAddress_buildingNumber        |
      | CreateCustomer        | POST   | /customers                                              | Individual_shippingAddress_apartmentNumber | /individual/shippingAddress/apartmentNumber  | "   "                                  | AddressApartmentNumberValidationRule  | Apartment Number cannot be empty or white space.                                | Address        | REF06_Validation_CreateCustomer_Individual_shippingAddress_apartmentNumber       |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | firstName                                  | /individual/firstName                        | "12345"                                | IndividualDataFirstNameValidationRule | First Name cannot be empty or white space and must contain at least one letter. | IndividualData | REF06_Validation_UpdateIndividualData_firstName                                  |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | lastName                                   | /individual/lastName                         | "12345"                                | IndividualDataLastNameValidationRule  | Last Name cannot be empty or white space and must contain at least one letter.  | IndividualData | REF06_Validation_UpdateIndividualData_lastName                                   |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | email                                      | /individual/email                            | "bad-email"                            | IndividualDataEmailValidationRule     | Email must be a valid format (address@domain.something).                        | IndividualData | REF06_Validation_UpdateIndividualData_email                                      |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | phone                                      | /individual/phone                            | "123456"                               | IndividualDataPhoneValidationRule     | Phone must contain only digits and be between 7 and 10 characters long.         | IndividualData | REF06_Validation_UpdateIndividualData_phone                                      |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | Individual_billingAddress_postalCode       | /individual/billingAddress/postalCode        | ""                                     | AddressPostalCodeValidationRule       | Postal Code cannot be empty or white space.                                     | Address        | REF06_Validation_UpdateIndividualData_Individual_billingAddress_postalCode       |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | Individual_billingAddress_city             | /individual/billingAddress/city              | "   "                                  | AddressCityValidationRule             | City cannot be empty or white space.                                            | Address        | REF06_Validation_UpdateIndividualData_Individual_billingAddress_city             |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | Individual_billingAddress_street           | /individual/billingAddress/street            | null                                   | AddressStreetValidationRule           | Street cannot be empty or white space.                                          | Address        | REF06_Validation_UpdateIndividualData_Individual_billingAddress_street           |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | Individual_billingAddress_buildingNumber   | /individual/billingAddress/buildingNumber    | ""                                     | AddressBuildingNumberValidationRule   | Building Number cannot be empty or white space.                                 | Address        | REF06_Validation_UpdateIndividualData_Individual_billingAddress_buildingNumber   |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | Individual_billingAddress_apartmentNumber  | /individual/billingAddress/apartmentNumber   | "   "                                  | AddressApartmentNumberValidationRule  | Apartment Number cannot be empty or white space.                                | Address        | REF06_Validation_UpdateIndividualData_Individual_billingAddress_apartmentNumber  |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | Individual_shippingAddress_postalCode      | /individual/shippingAddress/postalCode       | ""                                     | AddressPostalCodeValidationRule       | Postal Code cannot be empty or white space.                                     | Address        | REF06_Validation_UpdateIndividualData_Individual_shippingAddress_postalCode      |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | Individual_shippingAddress_city            | /individual/shippingAddress/city             | "   "                                  | AddressCityValidationRule             | City cannot be empty or white space.                                            | Address        | REF06_Validation_UpdateIndividualData_Individual_shippingAddress_city            |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | Individual_shippingAddress_street          | /individual/shippingAddress/street           | null                                   | AddressStreetValidationRule           | Street cannot be empty or white space.                                          | Address        | REF06_Validation_UpdateIndividualData_Individual_shippingAddress_street          |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | Individual_shippingAddress_buildingNumber  | /individual/shippingAddress/buildingNumber   | ""                                     | AddressBuildingNumberValidationRule   | Building Number cannot be empty or white space.                                 | Address        | REF06_Validation_UpdateIndividualData_Individual_shippingAddress_buildingNumber  |
      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | Individual_shippingAddress_apartmentNumber | /individual/shippingAddress/apartmentNumber  | "   "                                  | AddressApartmentNumberValidationRule  | Apartment Number cannot be empty or white space.                                | Address        | REF06_Validation_UpdateIndividualData_Individual_shippingAddress_apartmentNumber |
      | CreateCustomer        | POST   | /customers                                              | CompanyName                                | /companies/0/companyName                     | "   "                                  | CompanyDataCompanyNameValidationRule  | Company Name cannot be empty or white space.                                    | CompanyData    | REF06_Validation_CreateCustomer_CompanyName                                      |
      | CreateCustomer        | POST   | /customers                                              | TaxId                                      | /companies/0/taxId                           | "123456789"                            | CompanyDataTaxIdValidationRule        | Tax Id must be a valid Polish NIP containing exactly 10 digits.                 | CompanyData    | REF06_Validation_CreateCustomer_TaxId                                            |
      | CreateCustomer        | POST   | /customers                                              | Company_billingAddress_postalCode          | /companies/0/billingAddress/postalCode       | ""                                     | AddressPostalCodeValidationRule       | Postal Code cannot be empty or white space.                                     | Address        | REF06_Validation_CreateCustomer_Company_billingAddress_postalCode                |
      | CreateCustomer        | POST   | /customers                                              | Company_billingAddress_city                | /companies/0/billingAddress/city             | "   "                                  | AddressCityValidationRule             | City cannot be empty or white space.                                            | Address        | REF06_Validation_CreateCustomer_Company_billingAddress_city                      |
      | CreateCustomer        | POST   | /customers                                              | Company_billingAddress_street              | /companies/0/billingAddress/street           | null                                   | AddressStreetValidationRule           | Street cannot be empty or white space.                                          | Address        | REF06_Validation_CreateCustomer_Company_billingAddress_street                    |
      | CreateCustomer        | POST   | /customers                                              | Company_billingAddress_buildingNumber      | /companies/0/billingAddress/buildingNumber   | ""                                     | AddressBuildingNumberValidationRule   | Building Number cannot be empty or white space.                                 | Address        | REF06_Validation_CreateCustomer_Company_billingAddress_buildingNumber            |
      | CreateCustomer        | POST   | /customers                                              | Company_billingAddress_apartmentNumber     | /companies/0/billingAddress/apartmentNumber  | "   "                                  | AddressApartmentNumberValidationRule  | Apartment Number cannot be empty or white space.                                | Address        | REF06_Validation_CreateCustomer_Company_billingAddress_apartmentNumber           |
      | CreateCustomer        | POST   | /customers                                              | Company_shippingAddress_postalCode         | /companies/0/shippingAddress/postalCode      | ""                                     | AddressPostalCodeValidationRule       | Postal Code cannot be empty or white space.                                     | Address        | REF06_Validation_CreateCustomer_Company_shippingAddress_postalCode               |
      | CreateCustomer        | POST   | /customers                                              | Company_shippingAddress_city               | /companies/0/shippingAddress/city            | "   "                                  | AddressCityValidationRule             | City cannot be empty or white space.                                            | Address        | REF06_Validation_CreateCustomer_Company_shippingAddress_city                     |
      | CreateCustomer        | POST   | /customers                                              | Company_shippingAddress_street             | /companies/0/shippingAddress/street          | null                                   | AddressStreetValidationRule           | Street cannot be empty or white space.                                          | Address        | REF06_Validation_CreateCustomer_Company_shippingAddress_street                   |
      | CreateCustomer        | POST   | /customers                                              | Company_shippingAddress_buildingNumber     | /companies/0/shippingAddress/buildingNumber  | ""                                     | AddressBuildingNumberValidationRule   | Building Number cannot be empty or white space.                                 | Address        | REF06_Validation_CreateCustomer_Company_shippingAddress_buildingNumber           |
      | CreateCustomer        | POST   | /customers                                              | Company_shippingAddress_apartmentNumber    | /companies/0/shippingAddress/apartmentNumber | "   "                                  | AddressApartmentNumberValidationRule  | Apartment Number cannot be empty or white space.                                | Address        | REF06_Validation_CreateCustomer_Company_shippingAddress_apartmentNumber          |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | CompanyName                                | /companyName                                 | "   "                                  | CompanyDataCompanyNameValidationRule  | Company Name cannot be empty or white space.                                    | CompanyData    | REF06_Validation_AddCompany_CompanyName                                          |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | TaxId                                      | /taxId                                       | "12345a7890"                           | CompanyDataTaxIdValidationRule        | Tax Id must be a valid Polish NIP containing exactly 10 digits.                 | CompanyData    | REF06_Validation_AddCompany_TaxId                                                |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | Company_billingAddress_postalCode          | /billingAddress/postalCode                   | ""                                     | AddressPostalCodeValidationRule       | Postal Code cannot be empty or white space.                                     | Address        | REF06_Validation_AddCompany_Company_billingAddress_postalCode                    |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | Company_billingAddress_city                | /billingAddress/city                         | "   "                                  | AddressCityValidationRule             | City cannot be empty or white space.                                            | Address        | REF06_Validation_AddCompany_Company_billingAddress_city                          |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | Company_billingAddress_street              | /billingAddress/street                       | null                                   | AddressStreetValidationRule           | Street cannot be empty or white space.                                          | Address        | REF06_Validation_AddCompany_Company_billingAddress_street                        |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | Company_billingAddress_buildingNumber      | /billingAddress/buildingNumber               | ""                                     | AddressBuildingNumberValidationRule   | Building Number cannot be empty or white space.                                 | Address        | REF06_Validation_AddCompany_Company_billingAddress_buildingNumber                |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | Company_billingAddress_apartmentNumber     | /billingAddress/apartmentNumber              | "   "                                  | AddressApartmentNumberValidationRule  | Apartment Number cannot be empty or white space.                                | Address        | REF06_Validation_AddCompany_Company_billingAddress_apartmentNumber               |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | Company_shippingAddress_postalCode         | /shippingAddress/postalCode                  | ""                                     | AddressPostalCodeValidationRule       | Postal Code cannot be empty or white space.                                     | Address        | REF06_Validation_AddCompany_Company_shippingAddress_postalCode                   |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | Company_shippingAddress_city               | /shippingAddress/city                        | "   "                                  | AddressCityValidationRule             | City cannot be empty or white space.                                            | Address        | REF06_Validation_AddCompany_Company_shippingAddress_city                         |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | Company_shippingAddress_street             | /shippingAddress/street                      | null                                   | AddressStreetValidationRule           | Street cannot be empty or white space.                                          | Address        | REF06_Validation_AddCompany_Company_shippingAddress_street                       |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | Company_shippingAddress_buildingNumber     | /shippingAddress/buildingNumber              | ""                                     | AddressBuildingNumberValidationRule   | Building Number cannot be empty or white space.                                 | Address        | REF06_Validation_AddCompany_Company_shippingAddress_buildingNumber               |
      | AddCompany            | POST   | /customers/{customerId}/companies                       | Company_shippingAddress_apartmentNumber    | /shippingAddress/apartmentNumber             | "   "                                  | AddressApartmentNumberValidationRule  | Apartment Number cannot be empty or white space.                                | Address        | REF06_Validation_AddCompany_Company_shippingAddress_apartmentNumber              |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | CompanyName                                | /companyName                                 | "   "                                  | CompanyDataCompanyNameValidationRule  | Company Name cannot be empty or white space.                                    | CompanyData    | REF06_Validation_UpdateCompany_CompanyName                                       |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | TaxId                                      | /taxId                                       | "12345678901"                          | CompanyDataTaxIdValidationRule        | Tax Id must be a valid Polish NIP containing exactly 10 digits.                 | CompanyData    | REF06_Validation_UpdateCompany_TaxId                                             |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | Company_billingAddress_postalCode          | /billingAddress/postalCode                   | ""                                     | AddressPostalCodeValidationRule       | Postal Code cannot be empty or white space.                                     | Address        | REF06_Validation_UpdateCompany_Company_billingAddress_postalCode                 |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | Company_billingAddress_city                | /billingAddress/city                         | "   "                                  | AddressCityValidationRule             | City cannot be empty or white space.                                            | Address        | REF06_Validation_UpdateCompany_Company_billingAddress_city                       |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | Company_billingAddress_street              | /billingAddress/street                       | null                                   | AddressStreetValidationRule           | Street cannot be empty or white space.                                          | Address        | REF06_Validation_UpdateCompany_Company_billingAddress_street                     |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | Company_billingAddress_buildingNumber      | /billingAddress/buildingNumber               | ""                                     | AddressBuildingNumberValidationRule   | Building Number cannot be empty or white space.                                 | Address        | REF06_Validation_UpdateCompany_Company_billingAddress_buildingNumber             |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | Company_billingAddress_apartmentNumber     | /billingAddress/apartmentNumber              | "   "                                  | AddressApartmentNumberValidationRule  | Apartment Number cannot be empty or white space.                                | Address        | REF06_Validation_UpdateCompany_Company_billingAddress_apartmentNumber            |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | Company_shippingAddress_postalCode         | /shippingAddress/postalCode                  | ""                                     | AddressPostalCodeValidationRule       | Postal Code cannot be empty or white space.                                     | Address        | REF06_Validation_UpdateCompany_Company_shippingAddress_postalCode                |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | Company_shippingAddress_city               | /shippingAddress/city                        | "   "                                  | AddressCityValidationRule             | City cannot be empty or white space.                                            | Address        | REF06_Validation_UpdateCompany_Company_shippingAddress_city                      |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | Company_shippingAddress_street             | /shippingAddress/street                      | null                                   | AddressStreetValidationRule           | Street cannot be empty or white space.                                          | Address        | REF06_Validation_UpdateCompany_Company_shippingAddress_street                    |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | Company_shippingAddress_buildingNumber     | /shippingAddress/buildingNumber              | ""                                     | AddressBuildingNumberValidationRule   | Building Number cannot be empty or white space.                                 | Address        | REF06_Validation_UpdateCompany_Company_shippingAddress_buildingNumber            |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | Company_shippingAddress_apartmentNumber    | /shippingAddress/apartmentNumber             | "   "                                  | AddressApartmentNumberValidationRule  | Apartment Number cannot be empty or white space.                                | Address        | REF06_Validation_UpdateCompany_Company_shippingAddress_apartmentNumber           |
      | CreateAdmin           | POST   | /admins                                                 | ExternalBlank                              | /externalId                                  | "   "                                  | AdminExternalIdValidationRule         | ExternalId cannot be null or white space.                                       | Admin          | REF06_Validation_CreateAdmin_ExternalBlank                                       |
      | CreateAdmin           | POST   | /admins                                                 | FullName                                   | /fullName                                    | "Invalid@Name"                         | AdminFullNameValidationRule           | Full Name cannot be null, white space, or contain special characters (@#$%^&*). | Admin          | REF06_Validation_CreateAdmin_FullName                                            |
      | CreateAdmin           | POST   | /admins                                                 | Email                                      | /email                                       | "bad-email"                            | AdminEmailValidationRule              | Email must be a valid format (address@domain.something).                        | Admin          | REF06_Validation_CreateAdmin_Email                                               |
      | UpdateAdminProfile    | PUT    | /admins/{adminId}                                       | FullName                                   | /fullName                                    | "   "                                  | AdminFullNameValidationRule           | Full Name cannot be null, white space, or contain special characters (@#$%^&*). | Admin          | REF06_Validation_UpdateAdminProfile_FullName                                     |
      | UpdateAdminProfile    | PUT    | /admins/{adminId}                                       | Email                                      | /email                                       | "bad-email"                            | AdminEmailValidationRule              | Email must be a valid format (address@domain.something).                        | Admin          | REF06_Validation_UpdateAdminProfile_Email                                        |
      | AddProductToFavorites | POST   | /favorites/clients/77777777-7777-7777-7777-777777777777 | EmptyProduct                               | /productId                                   | "00000000-0000-0000-0000-000000000000" | FavoriteProductIdValidationRule       | ProductId cannot be an empty GUID.                                              | Favorite       | REF06_Validation_AddProductToFavorites_EmptyProduct                              |
      | CreateCustomer        | POST   | /customers                                              | Null_externalId                            | /externalId                                  | null                                   | CustomerExternalIdValidationRule      | ExternalId cannot be null or white space.                                       | Customer       | REF06_Validation_CreateCustomer_Null_externalId                                  |
      | CreateCustomer        | POST   | /customers                                              | Null_individual_firstName                  | /individual/firstName                        | null                                   | IndividualDataFirstNameValidationRule | First Name cannot be empty or white space and must contain at least one letter. | IndividualData | REF06_Validation_CreateCustomer_Null_individual_firstName                        |
      | CreateCustomer        | POST   | /customers                                              | Null_individual_email                      | /individual/email                            | null                                   | IndividualDataEmailValidationRule     | Email must be a valid format (address@domain.something).                        | IndividualData | REF06_Validation_CreateCustomer_Null_individual_email                            |
      | CreateCustomer        | POST   | /customers                                              | Null_individual_phone                      | /individual/phone                            | null                                   | IndividualDataPhoneValidationRule     | Phone must contain only digits and be between 7 and 10 characters long.         | IndividualData | REF06_Validation_CreateCustomer_Null_individual_phone                            |
      | CreateCustomer        | POST   | /customers                                              | Null_companies_0_companyName               | /companies/0/companyName                     | null                                   | CompanyDataCompanyNameValidationRule  | Company Name cannot be empty or white space.                                    | CompanyData    | REF06_Validation_CreateCustomer_Null_companies_0_companyName                     |
      | CreateCustomer        | POST   | /customers                                              | Null_companies_0_taxId                     | /companies/0/taxId                           | null                                   | CompanyDataTaxIdValidationRule        | Tax Id must be a valid Polish NIP containing exactly 10 digits.                 | CompanyData    | REF06_Validation_CreateCustomer_Null_companies_0_taxId                           |
      | CreateAdmin           | POST   | /admins                                                 | Null_externalId                            | /externalId                                  | null                                   | AdminExternalIdValidationRule         | ExternalId cannot be null or white space.                                       | Admin          | REF06_Validation_CreateAdmin_Null_externalId                                     |
      | CreateAdmin           | POST   | /admins                                                 | Null_fullName                              | /fullName                                    | null                                   | AdminFullNameValidationRule           | Full Name cannot be null, white space, or contain special characters (@#$%^&*). | Admin          | REF06_Validation_CreateAdmin_Null_fullName                                       |
      | CreateAdmin           | POST   | /admins                                                 | Null_email                                 | /email                                       | null                                   | AdminEmailValidationRule              | Email must be a valid format (address@domain.something).                        | Admin          | REF06_Validation_CreateAdmin_Null_email                                          |
      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | NullCompanyName                            | /companyName                                 | null                                   | CompanyDataCompanyNameValidationRule  | Company Name cannot be empty or white space.                                    | CompanyData    | REF06_Validation_UpdateCompany_NullCompanyName                                   |
      | CreateCustomer        | POST   | /customers                                              | PhoneTooLong                               | /individual/phone                            | "12345678901"                          | IndividualDataPhoneValidationRule     | Phone must contain only digits and be between 7 and 10 characters long.         | IndividualData | REF06_Validation_CreateCustomer_PhoneTooLong                                     |

  Scenario Outline: An invalid business identifier is rejected by <operationId>
    A route can match a syntactically valid identifier that the business policy rejects.
    The response has the exact rule error and the existing profiles, both history versions and Favorite stay unchanged.
    Given the business request uses "<operationId>" with method "<Method>" and path "<Path>"
    When I submit the business validation request
    Then the complete business validation problem contains exactly these errors
      | Name   | Message   | Entity   |
      | <Name> | <Message> | <Entity> |
    And every Users collection is unchanged after business rejection

    Examples:
      | operationId                | Method | Path                                                                                                  | Case                 | Name                             | Message                                   | Entity   | scenarioId                                                    |
      | UpdateIndividualData       | PUT    | /customers/00000000-0000-0000-0000-000000000000/individual                                            | EmptyCustomerId      | EmptyGuidRule                    | Guid cannot be empty.                     | Guid     | REF06_Validation_UpdateIndividualData_EmptyCustomerId         |
      | AddCompany                 | POST   | /customers/00000000-0000-0000-0000-000000000000/companies                                             | EmptyCustomerId      | EmptyGuidRule                    | Guid cannot be empty.                     | Guid     | REF06_Validation_AddCompany_EmptyCustomerId                   |
      | UpdateCompany              | PUT    | /customers/00000000-0000-0000-0000-000000000000/companies/{companyId}                                 | EmptyCustomerId      | EmptyGuidRule                    | Guid cannot be empty.                     | Guid     | REF06_Validation_UpdateCompany_EmptyCustomerId                |
      | UpdateCompany              | PUT    | /customers/{customerId}/companies/00000000-0000-0000-0000-000000000000                                | EmptyCompanyId       | EmptyGuidRule                    | Guid cannot be empty.                     | Guid     | REF06_Validation_UpdateCompany_EmptyCompanyId                 |
      | UpdateAdminProfile         | PUT    | /admins/00000000-0000-0000-0000-000000000000                                                          | EmptyAdminId         | EmptyGuidRule                    | Guid cannot be empty.                     | Guid     | REF06_Validation_UpdateAdminProfile_EmptyAdminId              |
      | GetFavoritesByClientId     | GET    | /favorites/clients/00000000-0000-0000-0000-000000000000                                               | EmptyClientId        | EmptyGuidRule                    | Guid cannot be empty.                     | Guid     | REF06_Validation_GetFavoritesByClientId_EmptyClientId         |
      | RemoveProductFromFavorites | DELETE | /favorites/clients/00000000-0000-0000-0000-000000000000/products/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa | EmptyClientId        | EmptyGuidRule                    | Guid cannot be empty.                     | Guid     | REF06_Validation_RemoveProductFromFavorites_EmptyClientId     |
      | RemoveProductFromFavorites | DELETE | /favorites/clients/77777777-7777-7777-7777-777777777777/products/00000000-0000-0000-0000-000000000000 | EmptyProductId       | EmptyGuidRule                    | Guid cannot be empty.                     | Guid     | REF06_Validation_RemoveProductFromFavorites_EmptyProductId    |
      | ClearClientFavorites       | DELETE | /favorites/clients/00000000-0000-0000-0000-000000000000                                               | EmptyClientId        | EmptyGuidRule                    | Guid cannot be empty.                     | Guid     | REF06_Validation_ClearClientFavorites_EmptyClientId           |
      | AddProductToFavorites      | POST   | /favorites/clients/00000000-0000-0000-0000-000000000000                                               | EmptyClientId        | FavoriteClientIdValidationRule   | ClientId cannot be an empty GUID.         | Favorite | REF06_Validation_AddProductToFavorites_EmptyClientId          |
      | GetCustomerByExternalId    | GET    | /customers/external/%20                                                                               | WhitespaceExternalId | CustomerExternalIdValidationRule | ExternalId cannot be null or white space. | Customer | REF06_Validation_GetCustomerByExternalId_WhitespaceExternalId |

  @allure.label.scenarioId:REF06_Validation_Customer_MultipleAndRepeatedErrors
  Scenario: Return all Customer errors including both identical address failures
    Several invalid fields include the same City rule on two distinct addresses; the exact error array retains both occurrences.
    Given the business request uses "CreateCustomer" with method "POST" and path "/customers"
    And I change only these JSON values
      | Pointer                          | JsonValue   |
      | /individual/email                | "bad-email" |
      | /individual/phone                | "123abc7"   |
      | /individual/billingAddress/city  | ""          |
      | /individual/shippingAddress/city | ""          |
      | /companies/0/companyName         | ""          |
      | /companies/0/taxId               | "bad-tax"   |
    When I submit the business validation request
    Then the complete business validation problem contains exactly these errors
      | Name                                 | Message                                                                 | Entity         |
      | IndividualDataEmailValidationRule    | Email must be a valid format (address@domain.something).                | IndividualData |
      | IndividualDataPhoneValidationRule    | Phone must contain only digits and be between 7 and 10 characters long. | IndividualData |
      | AddressCityValidationRule            | City cannot be empty or white space.                                    | Address        |
      | AddressCityValidationRule            | City cannot be empty or white space.                                    | Address        |
      | CompanyDataCompanyNameValidationRule | Company Name cannot be empty or white space.                            | CompanyData    |
      | CompanyDataTaxIdValidationRule       | Tax Id must be a valid Polish NIP containing exactly 10 digits.         | CompanyData    |
    And every Users collection is unchanged after business rejection

  @allure.label.scenarioId:REF06_Validation_Admin_MultipleErrors
  Scenario: Return every invalid Admin field without masking another rule
    External ID, full name and email fail together; all three exact rule errors are reported without a write.
    Given the business request uses "CreateAdmin" with method "POST" and path "/admins"
    And I change only these JSON values
      | Pointer     | JsonValue      |
      | /externalId | null           |
      | /fullName   | "Invalid@Name" |
      | /email      | "bad-email"    |
    When I submit the business validation request
    Then the complete business validation problem contains exactly these errors
      | Name                          | Message                                                                         | Entity |
      | AdminExternalIdValidationRule | ExternalId cannot be null or white space.                                       | Admin  |
      | AdminFullNameValidationRule   | Full Name cannot be null, white space, or contain special characters (@#$%^&*). | Admin  |
      | AdminEmailValidationRule      | Email must be a valid format (address@domain.something).                        | Admin  |
    And every Users collection is unchanged after business rejection

  @allure.label.scenarioId:REF06_Validation_CreateCustomer_DuplicateTax
  Scenario: Map duplicate company tax IDs during Customer creation to conflict
    The duplicate cause produces a complete 409 problem with no policy-error array and no persisted mutation.
    Given the business request uses "CreateCustomer" with method "POST" and path "/customers"
    And I change only these JSON values
      | Pointer            | JsonValue    |
      | /companies/1/taxId | "9876543210" |
    When I submit the business validation request
    Then the complete business conflict detail is "Resource CompanyData identified by id 9876543210 already exists in db. Error in action CreateCustomer."
    And every Users collection is unchanged after business rejection

  @allure.label.scenarioId:REF06_Validation_AddCompany_DuplicateTax
  Scenario: Map the collection-policy duplicate tax ID during company add to conflict
    The duplicate cause produces a complete 409 problem with no policy-error array and no persisted mutation.
    Given the business request uses "AddCompany" with method "POST" and path "/customers/{customerId}/companies"
    And I change only these JSON values
      | Pointer | JsonValue    |
      | /taxId  | "1234567890" |
    When I submit the business validation request
    Then the complete business conflict detail is "Resource CompanyData identified by id 1234567890 already exists in db. Error in action AddCompany."
    And every Users collection is unchanged after business rejection

  @allure.label.scenarioId:REF06_Validation_UpdateCompany_DuplicateTax
  Scenario: Reject another company tax ID using the update uniqueness guard
    The duplicate cause produces a complete 409 problem with no policy-error array and no persisted mutation.
    Given another distinct company exists for the business validation request
    Given the business request uses "UpdateCompany" with method "PUT" and path "/customers/{customerId}/companies/{companyId}"
    And I change only these JSON values
      | Pointer | JsonValue    |
      | /taxId  | "0987654321" |
    When I submit the business validation request
    Then the complete business conflict detail is "Resource CompanyData identified by id 0987654321 already exists in db. Error in action UpdateCompany."
    And every Users collection is unchanged after business rejection

  @allure.label.scenarioId:REF06_Validation_NullIndividual
  Scenario: Characterize NullIndividual as the existing safe mapping failure
    An explicit null object passes member-presence checks but the current mapping cannot handle it; this is a documented 500 gap rather than a policy-validation 400.
    Given the business request uses "CreateCustomer" with method "POST" and path "/customers"
    And I change only these JSON values
      | Pointer     | JsonValue |
      | /individual | null      |
    When I submit the business validation request
    Then the null object reports the existing safe server problem
    And every Users collection is unchanged after business rejection

  @allure.label.scenarioId:REF06_Validation_NullIndividualAddress
  Scenario: Characterize NullIndividualAddress as the existing safe mapping failure
    An explicit null object passes member-presence checks but the current mapping cannot handle it; this is a documented 500 gap rather than a policy-validation 400.
    Given the business request uses "UpdateIndividualData" with method "PUT" and path "/customers/{customerId}/individual"
    And I change only these JSON values
      | Pointer                    | JsonValue |
      | /individual/billingAddress | null      |
    When I submit the business validation request
    Then the null object reports the existing safe server problem
    And every Users collection is unchanged after business rejection

  @allure.label.scenarioId:REF06_Validation_NullCompanyAddress
  Scenario: Characterize NullCompanyAddress as the existing safe mapping failure
    An explicit null object passes member-presence checks but the current mapping cannot handle it; this is a documented 500 gap rather than a policy-validation 400.
    Given the business request uses "AddCompany" with method "POST" and path "/customers/{customerId}/companies"
    And I change only these JSON values
      | Pointer         | JsonValue |
      | /billingAddress | null      |
    When I submit the business validation request
    Then the null object reports the existing safe server problem
    And every Users collection is unchanged after business rejection

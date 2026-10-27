Feature: Generated Users request and response contracts

  @allure.label.scenarioId:REF10_Contracts_SourceMetadata
  Scenario: Published fields and typed examples match the actual serializer contract
    The generated OpenAPI is checked against CLR types, serializer-required members and nullable annotations.
    Every published field has a source description and a correctly typed example; documentation reads preserve all populated collections.
    Given versioned Users data exists before a technical operation
    And the published contract includes these fields
      | Schema                   | Field           | Type    | Required | Nullable | Format    |
      | CreateCustomerRequestDto | externalId      | string  | true     | false    | <none>    |
      | CreateCustomerRequestDto | individual      | object  | true     | false    | <none>    |
      | CreateCustomerRequestDto | companies       | array   | false    | true     | <none>    |
      | AddressRequestDto        | apartmentNumber | string  | true     | true     | <none>    |
      | AddressResponseDto       | apartmentNumber | string  | true     | true     | <none>    |
      | AddFavoriteRequestDto    | productId       | string  | true     | false    | uuid      |
      | AdminResponseDto         | isActive        | boolean | true     | false    | <none>    |
      | AdminResponseDto         | lastLoginAt     | string  | true     | false    | date-time |
      | CustomerResponseDto      | companies       | array   | true     | false    | <none>    |
      | CustomerResponseDto      | updatedAt       | string  | true     | false    | date-time |
      | FavoriteResponseDto      | id              | string  | true     | false    | uuid      |
      | FavoriteResponseDto      | addedAt         | string  | true     | false    | date-time |
      | ApiProblemDetails        | status          | integer | true     | false    | int32     |
      | ApiProblemDetails        | errors          | array   | true     | false    | <none>    |
      | FlowStepDescriptor       | order           | integer | true     | false    | int32     |
    When I read the generated Users OpenAPI document
    Then every generated DTO field matches serializer metadata and its typed example
    And the technical operation leaves all Users collections unchanged

  Scenario Outline: Accept the <representation> companies representation without changing the Customer profile
    Omitted, null and empty company collections normalize to an empty list without changing the individual profile.
    Web serializer member names are case-insensitive and unknown root or nested members are ignored; the response and real Insert/history snapshot stay complete.
    Given the serializer Customer JSON is
      """
      {
        "externalId": "contract-customer",
        "individual": {
          "firstName": "Anna",
          "lastName": "Kowalska",
          "email": "anna@example.com",
          "phone": "123456789",
          "billingAddress": {"postalCode":"00-001","city":"Warsaw","street":"Example Street","buildingNumber":"10","apartmentNumber":null},
          "shippingAddress": {"postalCode":"00-002","city":"Krakow","street":"Shipping Street","buildingNumber":"20","apartmentNumber":null}
        },
        "companies": []
      }
      """
    And the companies representation is "<representation>"
    When I submit the exact serializer Customer request
    Then the customer profile is created successfully
      | Field                                     | Value             |
      | StatusCode                                | 200               |
      | HasId                                     | true              |
      | HasUpdatedAt                              | true              |
      | ExternalId                                | contract-customer |
      | CompaniesCount                            | 0                 |
      | Individual.FirstName                      | Anna              |
      | Individual.LastName                       | Kowalska          |
      | Individual.Email                          | anna@example.com  |
      | Individual.Phone                          | 123456789         |
      | Individual.BillingAddress.PostalCode       | 00-001            |
      | Individual.BillingAddress.City             | Warsaw            |
      | Individual.BillingAddress.Street           | Example Street    |
      | Individual.BillingAddress.BuildingNumber   | 10                |
      | Individual.BillingAddress.ApartmentNumber  | <null>            |
      | Individual.ShippingAddress.PostalCode      | 00-002            |
      | Individual.ShippingAddress.City           | Krakow            |
      | Individual.ShippingAddress.Street          | Shipping Street   |
      | Individual.ShippingAddress.BuildingNumber  | 20                |
      | Individual.ShippingAddress.ApartmentNumber | <null>            |
    And the customer database contains the expected current and history snapshots
      | Field          | Value     |
      | CurrentBefore  | 0         |
      | HistoryBefore  | 0         |
      | VersionBefore  | none      |
      | CurrentAfter   | 1         |
      | HistoryAfter   | 1         |
      | VersionAfter   | 0         |
      | HistoryActions | Insert    |
      | UpdatedAt      | generated |

    Examples:
      | representation                | operationId    | scenarioId                           |
      | omitted                       | CreateCustomer | REF10_Serializer_Companies_Omitted   |
      | null                          | CreateCustomer | REF10_Serializer_Companies_Null      |
      | empty                         | CreateCustomer | REF10_Serializer_Companies_Empty     |
      | case-insensitive-with-unknown | CreateCustomer | REF10_Serializer_CaseAndUnknown      |

  @allure.label.scenarioId:REF10_Serializer_ApartmentMemberRequired
  Scenario: A nullable apartment number is still a required JSON member
    Explicit apartmentNumber null is accepted in the preceding cases, but omitting that member fails binding.
    A complete safe 400 problem identifies the missing nested member and every populated collection stays unchanged.
    Given representative Users data exists for framework rejection checks
    And I record all Users data before this framework request
    And the framework request is
      | Field       | Value                           |
      | Method      | POST                            |
      | Path        | /customers                      |
      | ContentType | application/json; charset=utf-8 |
      | Body        | <docstring>                     |
    And the exact framework body is
      """
      {
        "externalId": "contract-missing-apartment",
        "individual": {
          "firstName":"Anna","lastName":"Kowalska","email":"anna@example.com","phone":"123456789",
          "billingAddress":{"postalCode":"00-001","city":"Warsaw","street":"Example Street","buildingNumber":"10"},
          "shippingAddress":{"postalCode":"00-002","city":"Krakow","street":"Shipping Street","buildingNumber":"20","apartmentNumber":null}
        }
      }
      """
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                                                                                     |
      | Status            | 400                                                                                       |
      | Code              | invalid_json                                                                              |
      | Title             | Invalid JSON payload.                                                                     |
      | Detail            | JSON payload for AddressRequestDto is missing required properties: apartmentNumber.        |
      | MissingProperties | apartmentNumber                                                                           |
      | Allow             | []                                                                                        |
    And every Users collection is unchanged

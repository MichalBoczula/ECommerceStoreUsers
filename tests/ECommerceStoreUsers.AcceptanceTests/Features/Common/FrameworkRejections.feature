Feature: Framework rejections preserve Users data
  JSON binding and routing reject unprocessable requests before a business write.
  Every response is a complete safe problem, with paired exact-byte HTTP evidence.

  Background:
    Given representative Users data exists for framework rejection checks

  Scenario Outline: JSON write endpoints reject <Case> for <operationId>
    The database contains a Customer with a company, an Admin and a Favorite; both aggregates have version 1 and two history entries.
    A non-object body, null required body or absent required JSON members is rejected without changing any stored field.
    Given the framework request is
      | Field       | Value                           |
      | Method      | <Method>                        |
      | Path        | <Path>                          |
      | ContentType | application/json; charset=utf-8 |
      | Body        | <Body>                          |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value               |
      | Status            | <Status>            |
      | Code              | <Code>              |
      | Title             | <Title>             |
      | Detail            | <Detail>            |
      | MissingProperties | <MissingProperties> |
      | Allow             | []                  |
    And every Users collection is unchanged

    Examples:
      | Case            | operationId           | Method | Path                                                    | Body | Status | Code            | Title                 | Detail                                                                                                                        | MissingProperties                                   | scenarioId                                            |
      | array-root      | CreateCustomer        | POST   | /customers                                              | []   | 400    | invalid_json    | Invalid JSON payload. | The request body is not valid JSON.                                                                                           | []                                                  | REF07_Framework_CreateCustomer_array_root             |
      | null-body       | CreateCustomer        | POST   | /customers                                              | null | 400    | invalid_request | Invalid request.      | The request could not be processed.                                                                                           | []                                                  | REF07_Framework_CreateCustomer_null_body              |
      | missing-members | CreateCustomer        | POST   | /customers                                              | {}   | 400    | invalid_json    | Invalid JSON payload. | JSON payload for CreateCustomerRequestDto is missing required properties: externalId, individual.                             | externalId, individual                              | REF07_Framework_CreateCustomer_missing_members        |
      | array-root      | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | []   | 400    | invalid_json    | Invalid JSON payload. | The request body is not valid JSON.                                                                                           | []                                                  | REF07_Framework_UpdateIndividualData_array_root       |
      | null-body       | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | null | 400    | invalid_request | Invalid request.      | The request could not be processed.                                                                                           | []                                                  | REF07_Framework_UpdateIndividualData_null_body        |
      | missing-members | UpdateIndividualData  | PUT    | /customers/{customerId}/individual                      | {}   | 400    | invalid_json    | Invalid JSON payload. | JSON payload for UpdateIndividualDataRequestDto is missing required properties: individual.                                   | individual                                          | REF07_Framework_UpdateIndividualData_missing_members  |
      | array-root      | AddCompany            | POST   | /customers/{customerId}/companies                       | []   | 400    | invalid_json    | Invalid JSON payload. | The request body is not valid JSON.                                                                                           | []                                                  | REF07_Framework_AddCompany_array_root                 |
      | null-body       | AddCompany            | POST   | /customers/{customerId}/companies                       | null | 400    | invalid_request | Invalid request.      | The request could not be processed.                                                                                           | []                                                  | REF07_Framework_AddCompany_null_body                  |
      | missing-members | AddCompany            | POST   | /customers/{customerId}/companies                       | {}   | 400    | invalid_json    | Invalid JSON payload. | JSON payload for AddCompanyRequestDto is missing required properties: taxId, companyName, billingAddress, shippingAddress.    | taxId, companyName, billingAddress, shippingAddress | REF07_Framework_AddCompany_missing_members            |
      | array-root      | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | []   | 400    | invalid_json    | Invalid JSON payload. | The request body is not valid JSON.                                                                                           | []                                                  | REF07_Framework_UpdateCompany_array_root              |
      | null-body       | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | null | 400    | invalid_request | Invalid request.      | The request could not be processed.                                                                                           | []                                                  | REF07_Framework_UpdateCompany_null_body               |
      | missing-members | UpdateCompany         | PUT    | /customers/{customerId}/companies/{companyId}           | {}   | 400    | invalid_json    | Invalid JSON payload. | JSON payload for UpdateCompanyRequestDto is missing required properties: taxId, companyName, billingAddress, shippingAddress. | taxId, companyName, billingAddress, shippingAddress | REF07_Framework_UpdateCompany_missing_members         |
      | array-root      | CreateAdmin           | POST   | /admins                                                 | []   | 400    | invalid_json    | Invalid JSON payload. | The request body is not valid JSON.                                                                                           | []                                                  | REF07_Framework_CreateAdmin_array_root                |
      | null-body       | CreateAdmin           | POST   | /admins                                                 | null | 400    | invalid_request | Invalid request.      | The request could not be processed.                                                                                           | []                                                  | REF07_Framework_CreateAdmin_null_body                 |
      | missing-members | CreateAdmin           | POST   | /admins                                                 | {}   | 400    | invalid_json    | Invalid JSON payload. | JSON payload for CreateAdminRequestDto is missing required properties: externalId, fullName, email.                           | externalId, fullName, email                         | REF07_Framework_CreateAdmin_missing_members           |
      | array-root      | UpdateAdminProfile    | PUT    | /admins/{adminId}                                       | []   | 400    | invalid_json    | Invalid JSON payload. | The request body is not valid JSON.                                                                                           | []                                                  | REF07_Framework_UpdateAdminProfile_array_root         |
      | null-body       | UpdateAdminProfile    | PUT    | /admins/{adminId}                                       | null | 400    | invalid_request | Invalid request.      | The request could not be processed.                                                                                           | []                                                  | REF07_Framework_UpdateAdminProfile_null_body          |
      | missing-members | UpdateAdminProfile    | PUT    | /admins/{adminId}                                       | {}   | 400    | invalid_json    | Invalid JSON payload. | JSON payload for UpdateAdminProfileRequestDto is missing required properties: fullName, email.                                | fullName, email                                     | REF07_Framework_UpdateAdminProfile_missing_members    |
      | array-root      | AddProductToFavorites | POST   | /favorites/clients/77777777-7777-7777-7777-777777777777 | []   | 400    | invalid_json    | Invalid JSON payload. | The request body is not valid JSON.                                                                                           | []                                                  | REF07_Framework_AddProductToFavorites_array_root      |
      | null-body       | AddProductToFavorites | POST   | /favorites/clients/77777777-7777-7777-7777-777777777777 | null | 400    | invalid_request | Invalid request.      | The request could not be processed.                                                                                           | []                                                  | REF07_Framework_AddProductToFavorites_null_body       |
      | missing-members | AddProductToFavorites | POST   | /favorites/clients/77777777-7777-7777-7777-777777777777 | {}   | 400    | invalid_json    | Invalid JSON payload. | JSON payload for AddFavoriteRequestDto is missing required properties: productId.                                             | productId                                           | REF07_Framework_AddProductToFavorites_missing_members |

  @allure.label.scenarioId:REF07_Framework_Customer_StringType
  Scenario: Reject a numeric individual name before Customer creation
    A JSON number cannot bind to the string name; existing data and both histories remain unchanged.
    Given the framework request is
      | Field       | Value                           |
      | Field       | Value                           |
      | Method      | POST                            |
      | Path        | /customers                      |
      | ContentType | application/json; charset=utf-8 |
      | Body        | <docstring>                     |
    And the exact framework body is
      """
      {
        "externalId": "new-framework-customer",
        "individual": {
          "firstName": 42,
          "lastName": "Framework",
          "email": "framework@example.com",
          "phone": "123456789",
          "billingAddress": {
            "postalCode": "00-001",
            "city": "Warsaw",
            "street": "Framework Street",
            "buildingNumber": "10",
            "apartmentNumber": null
          },
          "shippingAddress": {
            "postalCode": "00-001",
            "city": "Warsaw",
            "street": "Framework Street",
            "buildingNumber": "10",
            "apartmentNumber": null
          }
        }
      }
      """
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                               |
      | Status            | 400                                 |
      | Code              | invalid_json                        |
      | Title             | Invalid JSON payload.               |
      | Detail            | The request body is not valid JSON. |
      | MissingProperties | []                                  |
      | Allow             | []                                  |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Company_StringType
  Scenario: Reject a boolean company name before adding a company
    A JSON boolean cannot bind to the company-name string and no new company is stored.
    Given the framework request is
      | Field       | Value                             |
      | Field       | Value                             |
      | Method      | POST                              |
      | Path        | /customers/{customerId}/companies |
      | ContentType | application/json; charset=utf-8   |
      | Body        | <docstring>                       |
    And the exact framework body is
      """
      {
        "taxId": "1234567890",
        "companyName": false,
        "billingAddress": {
          "postalCode": "00-001",
          "city": "Warsaw",
          "street": "Framework Street",
          "buildingNumber": "10",
          "apartmentNumber": null
        },
        "shippingAddress": {
          "postalCode": "00-001",
          "city": "Warsaw",
          "street": "Framework Street",
          "buildingNumber": "10",
          "apartmentNumber": null
        }
      }
      """
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                               |
      | Status            | 400                                 |
      | Code              | invalid_json                        |
      | Title             | Invalid JSON payload.               |
      | Detail            | The request body is not valid JSON. |
      | MissingProperties | []                                  |
      | Allow             | []                                  |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Admin_StringType
  Scenario: Reject an object email before an Admin update
    A JSON object cannot bind to the email string; the existing Admin and its two history entries are preserved.
    Given the framework request is
      | Field       | Value                                                                              |
      | Method      | PUT                                                                                |
      | Path        | /admins/{adminId}                                                                  |
      | ContentType | application/json; charset=utf-8                                                    |
      | Body        | {"fullName":"Updated Framework Admin","email":{"value":"INTERNAL_FAILURE_MARKER"}} |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                               |
      | Status            | 400                                 |
      | Code              | invalid_json                        |
      | Title             | Invalid JSON payload.               |
      | Detail            | The request body is not valid JSON. |
      | MissingProperties | []                                  |
      | Allow             | []                                  |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Customer_CollectionType
  Scenario: Reject an object instead of the Customer companies array
    The optional companies member is present with an object token instead of an array; no aggregate is created.
    Given the framework request is
      | Field       | Value                           |
      | Field       | Value                           |
      | Method      | POST                            |
      | Path        | /customers                      |
      | ContentType | application/json; charset=utf-8 |
      | Body        | <docstring>                     |
    And the exact framework body is
      """
      {
        "externalId": "new-framework-customer",
        "individual": {
          "firstName": "Updated",
          "lastName": "Framework",
          "email": "framework@example.com",
          "phone": "123456789",
          "billingAddress": {
            "postalCode": "00-001",
            "city": "Warsaw",
            "street": "Framework Street",
            "buildingNumber": "10",
            "apartmentNumber": null
          },
          "shippingAddress": {
            "postalCode": "00-001",
            "city": "Warsaw",
            "street": "Framework Street",
            "buildingNumber": "10",
            "apartmentNumber": null
          }
        },
        "companies": {}
      }
      """
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                               |
      | Status            | 400                                 |
      | Code              | invalid_json                        |
      | Title             | Invalid JSON payload.               |
      | Detail            | The request body is not valid JSON. |
      | MissingProperties | []                                  |
      | Allow             | []                                  |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Address_MissingMembers
  Scenario: Report all missing members of a nested billing address
    The first incomplete nested address omits postalCode and city; the response identifies both missing members exactly.
    Given the framework request is
      | Field       | Value                           |
      | Field       | Value                           |
      | Method      | POST                            |
      | Path        | /customers                      |
      | ContentType | application/json; charset=utf-8 |
      | Body        | <docstring>                     |
    And the exact framework body is
      """
      {
        "externalId": "new-framework-customer",
        "individual": {
          "firstName": "Updated",
          "lastName": "Framework",
          "email": "framework@example.com",
          "phone": "123456789",
          "billingAddress": {
            "street": "Framework Street",
            "buildingNumber": "10",
            "apartmentNumber": null
          },
          "shippingAddress": {
            "postalCode": "00-001",
            "city": "Warsaw",
            "street": "Framework Street",
            "buildingNumber": "10",
            "apartmentNumber": null
          }
        }
      }
      """
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                                                                                |
      | Status            | 400                                                                                  |
      | Code              | invalid_json                                                                         |
      | Title             | Invalid JSON payload.                                                                |
      | Detail            | JSON payload for AddressRequestDto is missing required properties: postalCode, city. |
      | MissingProperties | postalCode, city                                                                     |
      | Allow             | []                                                                                   |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_CompanyAddress_MissingMember
  Scenario: Report a missing street in the updated company shipping address
    All other company and address members are present, isolating the nested shipping-address deserialization failure.
    Given the framework request is
      | Field       | Value                                         |
      | Field       | Value                                         |
      | Method      | PUT                                           |
      | Path        | /customers/{customerId}/companies/{companyId} |
      | ContentType | application/json; charset=utf-8               |
      | Body        | <docstring>                                   |
    And the exact framework body is
      """
      {
        "taxId": "1234567890",
        "companyName": "Framework Company",
        "billingAddress": {
          "postalCode": "00-001",
          "city": "Warsaw",
          "street": "Framework Street",
          "buildingNumber": "10",
          "apartmentNumber": null
        },
        "shippingAddress": {
          "postalCode": "00-001",
          "city": "Warsaw",
          "buildingNumber": "10",
          "apartmentNumber": null
        }
      }
      """
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                                                                      |
      | Status            | 400                                                                        |
      | Code              | invalid_json                                                               |
      | Title             | Invalid JSON payload.                                                      |
      | Detail            | JSON payload for AddressRequestDto is missing required properties: street. |
      | MissingProperties | street                                                                     |
      | Allow             | []                                                                         |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Media_Absent
  @allure.label.branchId:users.branch.http.unsupported-media-type
  Scenario: Reject a nonempty JSON-shaped body without a content type
    A nonempty body with an unsupported or absent media type is rejected before Favorite binding.
    Given the framework request is
      | Field       | Value                                                   |
      | Method      | POST                                                    |
      | Path        | /favorites/clients/77777777-7777-7777-7777-777777777777 |
      | ContentType | <none>                                                  |
      | Body        | {"productId":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"}    |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                                      |
      | Status            | 415                                        |
      | Code              | unsupported_media_type                     |
      | Title             | Invalid request.                    |
      | Detail            | The request could not be processed. |
      | MissingProperties | []                                         |
      | Allow             | []                                         |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Media_Text
  @allure.label.branchId:users.branch.http.unsupported-media-type
  Scenario: Reject a text plain Favorite request
    A nonempty body with an unsupported or absent media type is rejected before Favorite binding.
    Given the framework request is
      | Field       | Value                                                   |
      | Method      | POST                                                    |
      | Path        | /favorites/clients/77777777-7777-7777-7777-777777777777 |
      | ContentType | text/plain; charset=utf-8                               |
      | Body        | {"productId":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"}    |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                                      |
      | Status            | 415                                        |
      | Code              | unsupported_media_type                     |
      | Title             | Unsupported media type.                    |
      | Detail            | The request content type is not supported. |
      | MissingProperties | []                                         |
      | Allow             | []                                         |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Route_FavoriteClient
  @allure.label.branchId:users.branch.http.route-not-found
  Scenario: Reject a non GUID Favorite client route segment
    A failed GUID route constraint selects no business endpoint; this is a routing 404 rather than a JSON-binding 400.
    Given the framework request is
      | Field       | Value                         |
      | Method      | GET                           |
      | Path        | /favorites/clients/not-a-guid |
      | ContentType | <none>                        |
      | Body        | <no-content>                  |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                               |
      | Status            | 404                                 |
      | Code              | route_not_found                     |
      | Title             | Route not found.                    |
      | Detail            | The requested route does not exist. |
      | MissingProperties | []                                  |
      | Allow             | []                                  |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Route_Customer
  @allure.label.branchId:users.branch.http.route-not-found
  Scenario: Reject a non GUID Customer route segment
    A failed GUID route constraint selects no business endpoint; this is a routing 404 rather than a JSON-binding 400.
    Given the framework request is
      | Field       | Value                            |
      | Method      | PUT                              |
      | Path        | /customers/not-a-guid/individual |
      | ContentType | <none>                           |
      | Body        | <no-content>                     |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                               |
      | Status            | 404                                 |
      | Code              | route_not_found                     |
      | Title             | Route not found.                    |
      | Detail            | The requested route does not exist. |
      | MissingProperties | []                                  |
      | Allow             | []                                  |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Route_Company
  @allure.label.branchId:users.branch.http.route-not-found
  Scenario: Reject a non GUID company route segment
    A failed GUID route constraint selects no business endpoint; this is a routing 404 rather than a JSON-binding 400.
    Given the framework request is
      | Field       | Value                                        |
      | Method      | PUT                                          |
      | Path        | /customers/{customerId}/companies/not-a-guid |
      | ContentType | <none>                                       |
      | Body        | <no-content>                                 |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                               |
      | Status            | 404                                 |
      | Code              | route_not_found                     |
      | Title             | Route not found.                    |
      | Detail            | The requested route does not exist. |
      | MissingProperties | []                                  |
      | Allow             | []                                  |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Route_FavoriteProduct
  @allure.label.branchId:users.branch.http.route-not-found
  Scenario: Reject a non GUID Favorite product route segment
    A failed GUID route constraint selects no business endpoint; this is a routing 404 rather than a JSON-binding 400.
    Given the framework request is
      | Field       | Value                                                                       |
      | Method      | DELETE                                                                      |
      | Path        | /favorites/clients/77777777-7777-7777-7777-777777777777/products/not-a-guid |
      | ContentType | <none>                                                                      |
      | Body        | <no-content>                                                                |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                               |
      | Status            | 404                                 |
      | Code              | route_not_found                     |
      | Title             | Route not found.                    |
      | Detail            | The requested route does not exist. |
      | MissingProperties | []                                  |
      | Allow             | []                                  |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Method_Favorites
  @allure.label.branchId:users.branch.http.method-not-allowed
  Scenario: Reject PUT on the Favorite collection route
    The route exists for DELETE, GET and POST; PUT returns a safe 405 and the complete allowed-method set.
    Given the framework request is
      | Field       | Value                                                   |
      | Method      | PUT                                                     |
      | Path        | /favorites/clients/77777777-7777-7777-7777-777777777777 |
      | ContentType | <none>                                                  |
      | Body        | <no-content>                                            |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                                                |
      | Status            | 405                                                  |
      | Code              | method_not_allowed                                   |
      | Title             | Method not allowed.                                  |
      | Detail            | The requested method is not supported by this route. |
      | MissingProperties | []                                                   |
      | Allow             | DELETE, GET, POST                                    |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Body_Absent
  Scenario: Reject an absent Favorite request body
    No content object is sent, preserving the distinction from an empty JSON content object.
    Given the framework request is
      | Field       | Value                                                   |
      | Method      | POST                                                    |
      | Path        | /favorites/clients/77777777-7777-7777-7777-777777777777 |
      | ContentType | <none>                                                  |
      | Body        | <no-content>                                            |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                               |
      | Status            | 400                                 |
      | Code              | invalid_request                     |
      | Title             | Invalid request.                    |
      | Detail            | The request could not be processed. |
      | MissingProperties | []                                  |
      | Allow             | []                                  |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Body_EmptyUpdate
  Scenario: Reject an empty Admin update body
    A zero-byte application/json content object cannot bind to a required Admin update body.
    Given the framework request is
      | Field       | Value                           |
      | Method      | PUT                             |
      | Path        | /admins/{adminId}               |
      | ContentType | application/json; charset=utf-8 |
      | Body        | <empty>                         |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                               |
      | Status            | 400                                 |
      | Code              | invalid_request                     |
      | Title             | Invalid request.                    |
      | Detail            | The request could not be processed. |
      | MissingProperties | []                                  |
      | Allow             | []                                  |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_Json_UnicodeMalformed
  Scenario: Reject malformed JSON with multibyte request bytes
    The unclosed JSON body contains Polish Unicode and a sensitive marker; the exact UTF-8 bytes are retained while diagnostics remain safe.
    Given the framework request is
      | Field       | Value                                                   |
      | Method      | POST                                                    |
      | Path        | /favorites/clients/77777777-7777-7777-7777-777777777777 |
      | ContentType | application/json; charset=utf-8                         |
      | Body        | {"productId":"żółć-INTERNAL_FAILURE_MARKER"             |
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                               |
      | Status            | 400                                 |
      | Code              | invalid_json                        |
      | Title             | Invalid JSON payload.               |
      | Detail            | The request body is not valid JSON. |
      | MissingProperties | []                                  |
      | Allow             | []                                  |
    And every Users collection is unchanged

  @allure.label.scenarioId:REF07_Framework_NullReference_BusinessBoundary
  Scenario: Explicit null reference reaches Admin policy validation
    JsonRequired enforces member presence and does not reject an explicitly null string by itself.
    A null externalId reaches the existing Admin rule and returns validation_failed rather than invalid_json; every collection stays unchanged.
    Given the framework request is
      | Field       | Value                           |
      | Method      | POST                            |
      | Path        | /admins                         |
      | ContentType | application/json; charset=utf-8 |
      | Body        | <docstring>                     |
    And the exact framework body is
      """
      {"externalId":null,"fullName":"Null Reference Admin","email":"null.reference@example.com"}
      """
    When I send the framework request
    Then the complete framework problem has these values
      | Field             | Value                                                                                                              |
      | Status            | 400                                                                                                                |
      | Code              | validation_failed                                                                                                  |
      | Title             | Validation failed.                                                                                                 |
      | Detail            | One or more validation errors occurred.                                                                            |
      | Errors            | [{"message":"ExternalId cannot be null or white space.","name":"AdminExternalIdValidationRule","entity":"Admin"}] |
      | MissingProperties | []                                                                                                                 |
      | Allow             | []                                                                                                                 |
    And every Users collection is unchanged

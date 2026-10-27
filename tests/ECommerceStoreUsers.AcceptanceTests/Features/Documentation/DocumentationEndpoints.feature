@allure.description:Ensures_documentation_endpoints_expose_every_registered_flow_and_validation_policy.
Feature: Documentation endpoints
  Runtime documentation must expose complete flow and validation metadata.

  @allure.label.scenarioId:REF07_GetFlowDocumentation_01
  Scenario: Get all flow descriptors
    A documentation consumer requests the service's published application flows.
    The response includes all twelve registered customer, administrator and favorites descriptors.
    Given versioned Users data exists before a technical operation
    When I request the flow documentation
    Then all 12 flow descriptors are returned with status 200
    And flow documentation has these exact descriptors
      | Descriptor | Flow | Steps |
      | GetCreateCustomerDescriptor | CreateCustomer | 7 |
      | GetCustomerByExternalIdDescriptor | GetCustomerByExternalId | 6 |
      | GetUpdateIndividualDataDescriptor | UpdateIndividualData | 10 |
      | GetAddCompanyDescriptor | AddCompany | 12 |
      | GetUpdateCompanyDescriptor | UpdateCompany | 15 |
      | GetGetAdminByExternalIdDescriptor | GetAdminByExternalId | 6 |
      | GetCreateAdminDescriptor | CreateAdmin | 7 |
      | GetUpdateAdminProfileDescriptor | UpdateAdminProfile | 9 |
      | GetGetFavoritesByClientIdDescriptor | GetFavoritesByClientId | 4 |
      | GetAddProductToFavoritesDescriptor | AddProductToFavorites | 7 |
      | GetRemoveProductFromFavoritesDescriptor | RemoveProductFromFavorites | 6 |
      | GetClearClientFavoritesDescriptor | ClearClientFavorites | 3 |
    And the technical operation leaves all Users collections unchanged

  @allure.label.scenarioId:REF07_GetValidationDocumentation_01
  Scenario: Get all validation descriptors
    A documentation consumer requests the service's published validation policies.
    The response includes all six registered policy descriptors.
    Given versioned Users data exists before a technical operation
    When I request the validation documentation
    Then all 6 validation policies are returned with status 200
    And validation documentation has these exact policies
      | Policy | Rules | Errors |
      | CustomerValidationPolicy | 13 | 14 |
      | IndividualDataValidationPolicy | 9 | 9 |
      | CompanyValidationPolicy | 7 | 7 |
      | AdminValidationPolicy | 3 | 3 |
      | EmptyGuidValidationPolicy | 1 | 1 |
      | FavoriteValidationPolicy | 2 | 2 |
    And the technical operation leaves all Users collections unchanged


  @allure.label.scenarioId:REF09_Documentation_OpenApi_ReadOnly
  Scenario: Generated OpenAPI is a read-only source contract
    A documentation consumer reads the actual generated Swagger JSON from the running application.
    The complete document equals the same host's bootstrap contract and contains all named operations; stronger property-level contract validation remains task 10.
    Given versioned Users data exists before a technical operation
    When I request the generated OpenAPI document
    Then the generated OpenAPI has these values
      | Field           | Value |
      | NamedOperations | 14    |
      | Schemas         | 23    |
    And the technical operation leaves all Users collections unchanged

Feature: Client cancellation before a Users persistence write
  A cancellation is a client transport outcome, not an invented HTTP error status.

  Scenario Outline: Cancelling <operationId> reaches the repository token before any write
    Valid version-one Customer/Admin profiles, their histories and an unrelated favorite exist.
    The request reaches its write-repository entry after normal binding, validation and reads; a test-only gate waits on the real propagated token before invoking any persistence write.
    Cancelling the HTTP request cancels that server token and the client receives a cancellation exception with no response. Complete database equality proves a pre-write cancellation, not rollback of an already started transaction.
    Given versioned Users data exists before a technical operation
    And the "<operationId>" write waits for request cancellation
    When I cancel the client request after its repository entry is reached
    Then cancellation has these observed values
      | Field              | Value    |
      | RepositoryEntries  | 1        |
      | ServerTokenCanCancel | true   |
      | ServerTokenCancelled | true   |
      | ClientCancelled    | true     |
      | ResponseObserved   | false    |
      | InnerWriteInvoked  | false    |
      | WriteEffect        | NoWrite  |
    And the technical operation leaves all Users collections unchanged

    Examples:
      | operationId            | scenarioId                              |
      | CreateCustomer         | REF09_Cancel_CreateCustomer              |
      | UpdateIndividualData   | REF09_Cancel_UpdateIndividualData        |
      | AddCompany             | REF09_Cancel_AddCompany                  |
      | UpdateCompany          | REF09_Cancel_UpdateCompany               |
      | CreateAdmin            | REF09_Cancel_CreateAdmin                 |
      | UpdateAdminProfile     | REF09_Cancel_UpdateAdminProfile          |
      | AddProductToFavorites  | REF09_Cancel_AddProductToFavorites       |
      | RemoveProductFromFavorites | REF09_Cancel_RemoveProductFromFavorites |
      | ClearClientFavorites   | REF09_Cancel_ClearClientFavorites        |

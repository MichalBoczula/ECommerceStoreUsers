Feature: Controlled write failures
  Every write operation returns a safe error when its repository rejects the write.
  Current and history documents must remain unchanged.

  Scenario Outline: <operationId> reports a safe 500 without changing stored data
    A customer or administrator write uses a repository decorator that throws before the real write begins.
    The safe server-error response leaves the complete current/history snapshots unchanged; this is a no-write fault, not a transaction-rollback demonstration.
    Given the "<operationId>" write will fail at the repository
    When I submit the failing "<operationId>" write
    Then the write reports a safe 500 and leaves current and history unchanged

    Examples:
      | operationId          | scenarioId                    |
      | CreateCustomer       | REF07_CreateCustomer_06       |
      | UpdateIndividualData | REF07_UpdateIndividualData_05 |
      | AddCompany           | REF07_AddCompany_06           |
      | UpdateCompany        | REF07_UpdateCompany_06        |
      | CreateAdmin          | REF07_CreateAdmin_05          |
      | UpdateAdminProfile   | REF07_UpdateAdminProfile_05   |

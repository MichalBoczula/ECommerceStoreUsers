Feature: Controlled write failures
  Every write operation returns a safe error when its repository rejects the write.
  Current and history documents must remain unchanged.

  Scenario Outline: <operationId> reports a safe 500 without changing stored data
    Given the "<operationId>" write will fail at the repository
    When I submit the failing "<operationId>" write
    Then the write reports a safe 500 and leaves current and history unchanged

    Examples:
      | operationId          |
      | CreateCustomer       |
      | UpdateIndividualData |
      | AddCompany           |
      | UpdateCompany        |
      | CreateAdmin          |
      | UpdateAdminProfile   |

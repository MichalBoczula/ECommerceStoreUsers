Feature: Real MongoDB uniqueness and transaction failures
  These scenarios execute the real repositories against the replica set.
  Repository decorators observe or arrange database effects and never manufacture the exception.

  Scenario Outline: <operationId> preserves a winner inserted after the external identifier lookup
    The first lookup sees no target external identifier, then a competing real repository insert commits a different complete profile.
    The losing create reaches MongoDB's ExternalId unique index after the application lookup and returns the same complete 409 resource_conflict contract as a sequential duplicate.
    Exactly one target current document and matching Insert history survive with no losing profile or orphan history.
    Given the "<operationId>" create loses an external identifier unique key race
    When I submit the observed "<operationId>" MongoDB write
    Then the confirmed create uniqueness conflict is reported as 409
    And only the committed race or rollback outcome remains
      | Field        | Value        |
      | Collection   | <collection> |
      | CurrentCount | 2            |
      | HistoryCount | 3            |
      | Version      | 0,1          |
      | Effect       | Winner       |

    Examples:
      | operationId      | collection | scenarioId                        |
      | CreateCustomer | customers  | REF08_CreateCustomer_UniqueKeyRace |
      | CreateAdmin    | admins     | REF08_CreateAdmin_UniqueKeyRace    |

  Scenario Outline: <operationId> rolls back its current write after a real history insertion failure
    A target-only unique index in the isolated history collection rejects the real history Insert after the repository has inserted or replaced current inside its transaction.
    An observer records MongoDB error 11000 from that history index and rethrows the same exception; no repository write is stubbed out.
    The API returns the complete safe 500 problem and every field in all five collections equals the snapshot taken before the failed transaction.
    Given the "<operationId>" transaction will fail inserting history in MongoDB
    When I submit the observed "<operationId>" MongoDB write
    Then the real MongoDB duplicate key error is safely reported as 500
    And only the committed race or rollback outcome remains
      | Field        | Value        |
      | Collection   | <collection> |
      | CurrentCount | <current>    |
      | HistoryCount | <history>    |
      | Version      | <versions>   |
      | Effect       | Rollback     |

    Examples:
      | operationId            | collection | current | history | versions | scenarioId                             |
      | CreateCustomer       | customers  | 1       | 3       | 1        | REF08_CreateCustomer_RealHistoryRollback |
      | UpdateIndividualData | customers  | 2       | 3       | 0,1      | REF08_UpdateIndividual_RealHistoryRollback |
      | AddCompany           | customers  | 2       | 3       | 0,1      | REF08_AddCompany_RealHistoryRollback    |
      | UpdateCompany        | customers  | 2       | 3       | 0,1      | REF08_UpdateCompany_RealHistoryRollback |
      | CreateAdmin          | admins     | 1       | 3       | 1        | REF08_CreateAdmin_RealHistoryRollback  |
      | UpdateAdminProfile   | admins     | 2       | 3       | 0,1      | REF08_UpdateAdmin_RealHistoryRollback  |


  Scenario Outline: <operationId> rejects one of two concurrent creates without losing the winner
    Two HTTP requests with distinct complete profiles use the same ExternalId and both finish their initial empty lookup before either proceeds to save.
    The real repositories execute concurrently: exactly one response is 200 and the other is the complete 409 resource_conflict or concurrency_conflict problem.
    The current profile and Insert history match the successful request and response in every field; the failed request contributes no data and unrelated collections remain unchanged.
    Given two valid "<operationId>" creates share one external identifier
      | Field      | First              | Second             |
      | ExternalId | ref08-concurrent   | ref08-concurrent   |
      | Name       | First              | Second             |
      | Email      | first@example.com  | second@example.com |
      | City       | Warsaw             | Krakow             |
      | TaxId      | 1234567890         | 9876543210         |
    When both create requests are submitted concurrently
    Then one create succeeds and the other returns the complete 409 conflict
    And only the committed race or rollback outcome remains
      | Field        | Value        |
      | Collection   | <collection> |
      | CurrentCount | 2            |
      | HistoryCount | 3            |
      | Version      | 0,1          |
      | Effect       | Winner       |

    Examples:
      | operationId    | collection | scenarioId                           |
      | CreateCustomer | customers  | REF08_CreateCustomer_ConcurrentHttp409 |
      | CreateAdmin    | admins     | REF08_CreateAdmin_ConcurrentHttp409    |

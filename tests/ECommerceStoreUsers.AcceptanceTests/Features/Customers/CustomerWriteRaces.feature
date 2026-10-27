Feature: Conditional customer writes

  @allure.label.scenarioId:REF08_CustomerUpdate_409_ConcurrentChange
  Scenario: REF08_CustomerUpdate_409_ConcurrentChange
    Another write changes the customer after the update reads its current version.
    The losing individual-data update returns a conflict and adds no losing update to history.
    Given a customer update loses a concurrent change race
    When the raced individual update is submitted
    Then the raced update returns 409 with no additional history
    And only the committed race or rollback outcome remains
      | Field        | Value      |
      | Collection   | customers  |
      | CurrentCount | 1          |
      | HistoryCount | 2          |
      | Version      | 1          |
      | Effect       | Winner     |

  @allure.label.scenarioId:REF08_CustomerUpdate_404_DeletedAfterRead
  Scenario: REF08_CustomerUpdate_404_DeletedAfterRead
    Another operation deletes the customer after the update reads it.
    The individual-data update returns not found and adds no update history.
    Given a customer update loses a concurrent delete race
    When the raced individual update is submitted
    Then the raced update returns 404 with no additional history
    And only the committed race or rollback outcome remains
      | Field        | Value      |
      | Collection   | customers  |
      | CurrentCount | 0          |
      | HistoryCount | 1          |
      | Version      | absent     |
      | Effect       | Delete     |


  Scenario Outline: <operationId> preserves the winner of a <race> company race
    A valid company write reads a customer with two distinct companies before a competing write commits or a direct database delete removes it.
    SameTaxId and DifferentTaxId both use a stale aggregate version and must report concurrency_conflict, rather than the sequential duplicate-tax-ID resource_conflict.
    The complete winning individual, both original companies, changed or added company, versions and all history metadata survive; the loser contributes no fields.
    Given a "<operationId>" write loses a "<race>" company race
    When the raced individual update is submitted
    Then the raced update returns <status> with no additional history
    And only the committed race or rollback outcome remains
      | Field        | Value      |
      | Collection   | customers  |
      | CurrentCount | <current>  |
      | HistoryCount | <history>  |
      | Version      | <version>  |
      | Effect       | <effect>   |

    Examples:
      | operationId     | race           | status | current | history | version | effect | scenarioId                               |
      | AddCompany    | DifferentTaxId | 409    | 1       | 2       | 1       | Winner | REF08_AddCompany_ConcurrentDifferentTaxId |
      | AddCompany    | SameTaxId      | 409    | 1       | 2       | 1       | Winner | REF08_AddCompany_ConcurrentSameTaxId      |
      | AddCompany    | Delete         | 404    | 0       | 1       | absent  | Delete | REF08_AddCompany_DeletedAfterRead         |
      | UpdateCompany | DifferentTaxId | 409    | 1       | 2       | 1       | Winner | REF08_UpdateCompany_ConcurrentDifferentTaxId |
      | UpdateCompany | SameTaxId      | 409    | 1       | 2       | 1       | Winner | REF08_UpdateCompany_ConcurrentSameTaxId      |
      | UpdateCompany | Delete         | 404    | 0       | 1       | absent  | Delete | REF08_UpdateCompany_DeletedAfterRead         |

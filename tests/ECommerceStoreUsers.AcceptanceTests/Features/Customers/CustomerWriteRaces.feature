Feature: Conditional customer writes

  Scenario: REF08_CustomerUpdate_409_ConcurrentChange
    Given a customer update loses a concurrent change race
    When the raced individual update is submitted
    Then the raced update returns 409 with no additional history

  Scenario: REF08_CustomerUpdate_404_DeletedAfterRead
    Given a customer update loses a concurrent delete race
    When the raced individual update is submitted
    Then the raced update returns 404 with no additional history

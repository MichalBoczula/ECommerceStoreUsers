Feature: Conditional customer writes

  @allure.label.scenarioId:REF08_CustomerUpdate_409_ConcurrentChange
  Scenario: REF08_CustomerUpdate_409_ConcurrentChange
    Another write changes the customer after the update reads its current version.
    The losing individual-data update returns a conflict and adds no losing update to history.
    Given a customer update loses a concurrent change race
    When the raced individual update is submitted
    Then the raced update returns 409 with no additional history

  @allure.label.scenarioId:REF08_CustomerUpdate_404_DeletedAfterRead
  Scenario: REF08_CustomerUpdate_404_DeletedAfterRead
    Another operation deletes the customer after the update reads it.
    The individual-data update returns not found and adds no update history.
    Given a customer update loses a concurrent delete race
    When the raced individual update is submitted
    Then the raced update returns 404 with no additional history

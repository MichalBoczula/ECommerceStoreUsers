Feature: Conditional admin writes

  @allure.label.scenarioId:REF08_AdminUpdate_409_ConcurrentChange
  Scenario: REF08_AdminUpdate_409_ConcurrentChange
    Another write changes the administrator after the update reads its current version.
    The losing profile update returns a conflict while the winning version and history remain.
    Given an admin update loses a concurrent change race
    When the raced admin profile update is submitted
    Then the raced admin update returns 409 with consistent history

  @allure.label.scenarioId:REF08_AdminUpdate_404_DeletedAfterRead
  Scenario: REF08_AdminUpdate_404_DeletedAfterRead
    Another operation deletes the administrator after the update reads it.
    The profile update returns not found and adds no update history.
    Given an admin update loses a concurrent delete race
    When the raced admin profile update is submitted
    Then the raced admin update returns 404 with consistent history

  @allure.label.scenarioId:REF08_AdminUpdate_200_UnchangedProfile
  Scenario: REF08_AdminUpdate_200_UnchangedProfile
    An administrator submits the same full name and email as the stored profile.
    The successful no-op returns the existing profile without a new version or history entry.
    Given an admin profile is unchanged on update
    When the raced admin profile update is submitted
    Then the raced admin update returns 200 with consistent history

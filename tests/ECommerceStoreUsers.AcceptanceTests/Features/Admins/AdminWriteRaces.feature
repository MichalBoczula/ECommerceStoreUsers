Feature: Conditional admin writes

  Scenario: REF08_AdminUpdate_409_ConcurrentChange
    Given an admin update loses a concurrent change race
    When the raced admin profile update is submitted
    Then the raced admin update returns 409 with consistent history

  Scenario: REF08_AdminUpdate_404_DeletedAfterRead
    Given an admin update loses a concurrent delete race
    When the raced admin profile update is submitted
    Then the raced admin update returns 404 with consistent history

  Scenario: REF08_AdminUpdate_200_UnchangedProfile
    Given an admin profile is unchanged on update
    When the raced admin profile update is submitted
    Then the raced admin update returns 200 with consistent history

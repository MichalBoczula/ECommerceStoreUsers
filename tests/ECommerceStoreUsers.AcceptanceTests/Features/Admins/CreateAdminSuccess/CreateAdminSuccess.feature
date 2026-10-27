@allure.description:Ensures_creating_an_admin_profile_returns_a_success_response_with_the_created_payload_fields.
Feature: Create admin profile

  @allure.label.scenarioId:REF07_CreateAdmin_03
  Scenario: Create admin profile returns created admin data
    A new administrator registers an external identity, full name and email address.
    The create request returns the generated administrator identifier and profile data.
    Given I have a valid create admin request
      | Field      | Value             |
      | ExternalId | auth-678901234    |
      | FullName   | Jane Smith        |
      | Email      | jane.smith@db.com |
    When I submit the create admin request
    Then the admin profile is created successfully
      | Field          | Value             |
      | StatusCode     | 200               |
      | HasId          | true              |
      | ExternalId     | auth-678901234    |
      | FullName       | Jane Smith        |
      | Email          | jane.smith@db.com |
      | IsActive       | true              |
      | HasLastLoginAt | true              |
    And the admin database contains the expected current and history snapshots
      | Field          | Value     |
      | CurrentBefore  | 0         |
      | HistoryBefore  | 0         |
      | VersionBefore  | none      |
      | CurrentAfter   | 1         |
      | HistoryAfter   | 1         |
      | VersionAfter   | 0         |
      | HistoryActions | Insert    |
      | WriteEffect    | Insert    |
      | LastLoginAt    | generated |

  @allure.label.scenarioId:REF06_Validation_AdminBoundary_EmptyGuidAndNumericName
  Scenario: Accept an empty GUID external string and numeric Admin name
    Admin ExternalId only rejects null/whitespace and FullName only rejects blanks or the listed special characters.
    The accepted boundary response and complete version-0 current/Insert history must match the explicit fields.
    Given I have a valid create admin request
      | Field      | Value             |
      | ExternalId | 00000000-0000-0000-0000-000000000000    |
      | FullName   | 12345        |
      | Email      | jane.smith@db.com |
    When I submit the create admin request
    Then the admin profile is created successfully
      | Field          | Value             |
      | StatusCode     | 200               |
      | HasId          | true              |
      | ExternalId     | 00000000-0000-0000-0000-000000000000    |
      | FullName       | 12345        |
      | Email          | jane.smith@db.com |
      | IsActive       | true              |
      | HasLastLoginAt | true              |
    And the admin database contains the expected current and history snapshots
      | Field          | Value     |
      | CurrentBefore  | 0         |
      | HistoryBefore  | 0         |
      | VersionBefore  | none      |
      | CurrentAfter   | 1         |
      | HistoryAfter   | 1         |
      | VersionAfter   | 0         |
      | HistoryActions | Insert    |
      | WriteEffect    | Insert    |
      | LastLoginAt    | generated |

# USERS-BDD-04: Admin success and unchanged-profile evidence

The original create, get, update and unchanged-profile cases retain their IDs,
titles, input values and HTTP outcomes. A new repeat-after-change case proves
that a successful no-op also preserves version 1 and two existing snapshots.
The suite now has 85 BDD cases, 65 declarations in 42 features, and 75 matrix
links covering the same 54 operation/status pairs. The acceptance-project
total is 104, including the existing 19 standalone cases.

## Covered behavior

| Case | Expected stored transition |
| --- | --- |
| CreateAdmin | One current Admin at version 0 and one complete Insert history snapshot; active profile and generated LastLoginAt. |
| GetAdminByExternalId | Complete response and unchanged current/history data, IDs, version and LastLoginAt. |
| UpdateAdminProfile | Name/email change, version 0→1 and one resulting Update snapshot; identity, IsActive and LastLoginAt retained. |
| Original unchanged-profile PUT | Full current/history projections remain unchanged at version 0 with one Insert snapshot. |
| Repeat a previously applied PUT | The Given setup proves a real 0→1 update; the repeated PUT retains version 1 and the complete Insert/Update history. |

Every response explicitly checks all six DTO fields: Id, ExternalId, FullName,
Email, IsActive and LastLoginAt, plus HTTP status/media type. Missing Gherkin
expectations fail instead of comparing received values with themselves. These
fixtures use profiles created active by the public create endpoint; they do
not claim coverage of an inactive-account lifecycle or perform a login.

## Structure and attachments

Thin success bindings use a scenario-local `AdminSuccessState` helper, following
the Customer evidence pattern. Original setup/create/update values are kept.
Each case includes a persistence table with current/history counts before and
after, versions, action sequence, write effect and LastLoginAt behavior.

The original no-op fixture generates an external ID in its setup request.
Its response table uses `<setup-external-id>` to refer to that recorded input;
it never derives the expected external ID from the response being checked.

Task 2 captures every actual HTTP request/response. Additional correlated JSON
attachments include Admin setup request/database, database before/after,
response/persistence expectations and a verification summary. Resulting state
is captured immediately after HTTP completion, before response assertions.
The summary includes the measured request time bounds. The repeat case labels
its first, real write separately as **Admin prior update**.

Snapshot projections contain all known document fields. They show GUIDs, UTC
dates and readable Insert/Update actions. Every retained history entry is
compared in full, including ID, AdminId, action, version and ChangedAt. Latest
snapshot data must equal the current profile. GET and no-op cases compare the
complete before/after projections, so even a changed history ID or ChangedAt
fails the check.

MongoDB stores milliseconds while a create response may retain finer .NET
ticks. Only the HTTP LastLoginAt comparison is normalized to milliseconds.
Generated LastLoginAt and new history ChangedAt must fall within the measured
request window. GET, profile updates and repeated PUTs retain LastLoginAt.
Version remains an internal persistence concern; no public DTO field is added.

## Verification and scope

Run the existing source and acceptance stages:

```bash
bash scripts/ci.sh source
bash scripts/ci.sh test acceptance
```

The PR records full CI and inspection of all five Admin success/no-op cases
in the fresh bundle, including the real prior write in the repeat scenario.
Local checks cover source links, inventory, architecture and whitespace; the
workspace has no .NET SDK or Docker daemon, so runtime checks use existing CI.
The original 81-case baseline and all three Customer additions are retained.

No application, DTO, repository, database schema, dependency, CI job or Azure
resource changes. Existing concurrent-change/delete paths remain for task 8;
only their shared fixture's no-op branch uses the new success assertions.
LiveDocs selection and Azure rollout remain separate until task 12.

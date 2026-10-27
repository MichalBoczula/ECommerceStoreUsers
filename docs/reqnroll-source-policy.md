# Acceptance source and generated code

Reqnroll `.feature` files and their step definitions are the source of the
acceptance suite. Reqnroll generates `.feature.cs` during the build; generated
files are ignored by Git. Do not edit or commit them.

The USERS-BDD-01 baseline contains 42 `.feature` files, 61 declared scenarios
(including scenario outlines), and 81 expanded BDD cases. Six additional xUnit
facts bring the baseline acceptance-project total to 87; those facts are not
Allure BDD scenarios. See [the inventory and provenance](bdd-01-baseline.md)
and [the task backlog](bdd-backlog.md). The source gate preserves every original
scenario/example while allowing additions. A deliberate rename, move or example
change needs an explicitly reviewed baseline migration; do not regenerate the
baseline merely to make a missing case pass. No feature is explicitly removed from
`ReqnrollFeatureFile`. In particular, the admin and customer creation success
scenarios are enabled. For a clean checkout, run the acceptance test project
from the repository root with the command in `AGENTS.md`. Inspect the test
discovery/output for the expected scenarios, including both creation flows.

Task 2 adds descriptions, scenario/instance identities and shared HTTP evidence
without replacing those original cases. Its 13 standalone recorder-test cases
bring the current expected acceptance total to 100: 81 BDD plus 19 non-BDD test
cases. The baseline comparison excludes only the added `scenarioId` metadata
column, retaining all original behavioral example values. See
[the evidence contract](bdd-02-http-evidence.md) for attachment formats, client
coverage, exclusions and verification.

Task 3 retains those original cases and adds three Customer preservation cases:
84 expanded BDD cases, 64 declarations in the same 42 feature files, and 103
acceptance-project cases including the 19 standalone cases. See
[Customer success evidence](bdd-03-customers.md) for complete profile and
current/history assertions, fixture field names and timestamp behavior.

Task 4 strengthens the three Admin successes and original no-op, adding one
repeat-after-change case: 85 expanded BDD cases, 65 declarations in 42 feature
files, and 104 acceptance-project cases (19 standalone). See
[Admin success evidence](bdd-04-admins.md) for complete response/current/history
checks and unchanged-version/history assertions at versions 0 and 1.

Task 5 strengthens the five original Favorites successes and adds six collection/
isolation cases: 91 expanded BDD cases, 71 declarations in 42 feature files,
and 110 acceptance-project cases (19 standalone). See
[Favorites evidence](bdd-05-favorites.md) for complete persisted fields, empty
results and no-ops, client isolation and the actual timestamp ordering contract.

Task 7 adds 37 framework rejections and one explicit-null business-boundary
control, and strengthens the original framework/missing-member cases. The
inventory is now 129 expanded BDD cases, 89 declarations in 43 features,
and 150 acceptance-project cases (21 standalone, including two route-checker
regressions). See [framework rejection evidence](bdd-07-framework.md).
Task 7 itself does not establish complete business-validation coverage.

The task-6 follow-up adds 105 business-rule/boundary cases and strengthens
18 original ordinary validation/conflict cases. The inventory is now 234
expanded BDD cases, 101 declarations in 44 features, 217 named-operation links
and 255 acceptance-project cases (21 standalone). See
[business-validation evidence](bdd-06-business-validation.md) for all active
rule signatures, accepted boundaries and mapped conflicts.

The [required-object follow-up](bdd-06-null-object-fix.md) fixes the three null-object
500 cases and adds ten cases across shared mapping paths: 244 BDD cases,
111 declarations in 44 features, 227 links and 265 acceptance-project cases
(21 standalone). Application unit tests add three direct guard cases.

The MongoDB.Driver 3.7.1 dependency brings in SharpCompress and Snappier.
Infrastructure pins their versions to 0.48.1 and 1.3.1 respectively, which
are newer than the driver's minimums. Benchmarks reference Infrastructure,
so their redundant direct pins were removed. Keep the Infrastructure pins
until a package graph and security review justify changing them.



Task 8 adds real MongoDB transaction and full winning-state evidence; see
[bdd-08-atomicity.md](bdd-08-atomicity.md). Current inventory: **258 expanded BDD
cases, 114 declarations in 45 features, 241 named-operation links and 279
acceptance-project cases** (21 standalone unchanged). Deterministic HTTP
interleavings are distinguished from actual overlapping repository integration
tests, and pre-repository exceptions from real history-insert rollback. The
Customer/Admin create unique-key race currently returns safe 500 and is recorded
as a production follow-up rather than mislabeled as the sequential 409.

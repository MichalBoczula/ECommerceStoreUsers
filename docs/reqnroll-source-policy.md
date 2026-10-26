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

The MongoDB.Driver 3.7.1 dependency brings in SharpCompress and Snappier.
Infrastructure pins their versions to 0.48.1 and 1.3.1 respectively, which
are newer than the driver's minimums. Benchmarks reference Infrastructure,
so their redundant direct pins were removed. Keep the Infrastructure pins
until a package graph and security review justify changing them.


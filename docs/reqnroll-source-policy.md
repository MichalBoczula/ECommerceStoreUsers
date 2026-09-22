# Acceptance source and generated code

Reqnroll `.feature` files and their step definitions are the source of the
acceptance suite. Reqnroll generates `.feature.cs` during the build; generated
files are ignored by Git. Do not edit or commit them.

The suite currently contains 37 `.feature` files and 48 declared scenarios
(including scenario outlines). No feature is explicitly removed from
`ReqnrollFeatureFile`. In particular, the admin and customer creation success
scenarios are enabled. For a clean checkout, run the acceptance test project
from the repository root with the command in `AGENTS.md`. Inspect the test
discovery/output for the expected scenarios, including both creation flows.

The MongoDB.Driver 3.7.1 dependency brings in SharpCompress and Snappier.
Infrastructure pins their versions to 0.48.1 and 1.3.1 respectively, which
are newer than the driver's minimums. Benchmarks reference Infrastructure,
so their redundant direct pins were removed. Keep the Infrastructure pins
until a package graph and security review justify changing them.

# USERS-BDD-01: BDD inventory and coverage guard

## Baseline and provenance

The starting point is Users master commit
`112c4be06a79ef1f0653e1e2d8d6de0538e7d611` and its successful
[CI run 40210725093](https://github.com/MichalBoczula/ECommerceStoreUsers/actions/runs/40210725093).
The actual archived bundle is SHA-256
`43ea9b08bf925ec1138b624d607b4f36c8ef95776640c5a6941115988efbc6f3`
(334786 bytes), generated at `2026-10-25T16:16:52Z`.
Its 537 manifest files and 329 referenced Allure attachments were checked;
all 219 JSON attachments parse. All 81 BDD results passed.

[`bdd-baseline.json`](bdd-baseline.json) records every source scenario and
every outline example's complete column values at that commit. It is a fixed
regression inventory, not an API specification or a generated report.

| Source area | Feature files | Declared scenarios/outlines | Expanded BDD cases |
| --- | ---: | ---: | ---: |
| Customers | 23 | 24 | 24 |
| Admins | 12 | 14 | 14 |
| Favorites | 1 | 14 | 14 |
| Common | 5 | 7 | 27 |
| Documentation | 1 | 2 | 2 |
| Total | 42 | 61 | 81 |

The acceptance project also contains six non-BDD xUnit facts: three Mongo
readiness tests, two startup/cancellation tests and one OpenAPI export test.
The baseline CI therefore reports **87 acceptance-project tests**, while
Allure contains **81 BDD cases**. Those numbers describe different inventories.

## What the original checks missed

The old endpoint parser stopped at `.WithName(...)`. Every current endpoint
declares `.Produces(...)` after that call, so all 14 operations had an empty
parsed response-status set and the missing-status assertion was vacuous.
Reading complete registrations, including nested generic DTOs, finds **54
distinct operation/status pairs**. All 54 already have coverage in the **71
matrix rows**; task 1 fixes the guard rather than claiming missing scenarios
were added.

The updated existing source and contract scripts reject:

- duplicate endpoint operation names and registrations with no parsed status;
- missing/stale matrix operations, routes or statuses;
- missing features/scenarios and outline values found only in another scenario;
- removal or replacement of an original expanded BDD case, even if the total
  count stays unchanged; additional cases are allowed;
- exported business/documentation operations without links, including unnamed
  operations, and missing exported operations;
- exported response statuses without acceptance cases and matrix cases without
  exported responses; existing media/DTO/204-body checks remain.

Only GET `/health`, `/health/live` and `/health/ready` are explicitly outside
the OpenAPI business/documentation matrix. Framework 404/405/415 are exercised
in the technical BDD scenarios rather than declared as business outcomes.

The source reader recognizes the literal English scenario/Examples and
`group.Map...`/`.WithName`/`.Produces(StatusCodes...)` patterns used by this
repository. It is not a general C# or Gherkin compiler. The baseline guard
proves source presence, not test execution, attachment completeness or all
business behavior. Actual CI results and report bundles remain necessary.
No endpoints, DTOs, persistence behavior or feature bodies change in task 1.

## Verification

The existing `bash scripts/ci.sh source` entry point runs the inventory/matrix
gate and the extended `scripts/test-openapi-contract.py` regressions. The
regressions deliberately remove cases, add stale links/statuses and replace
outline values to prove failures, and exercise valid metadata/table patterns.

```bash
bash scripts/ci.sh source
python3 scripts/check-openapi-contract.py path/to/exported-openapi.json
```

The task was checked locally with the actual baseline exported OpenAPI and
the full source stage. Full .NET, MongoDB, security, image and fresh report
checks run through the existing PR CI; the PR records their results. This
workspace has no .NET SDK or Docker daemon. There is no Azure deployment or
LiveDocs selection change in this task.

## Continuing the work

Follow [the 12-task backlog](bdd-backlog.md). The baseline has 23 cases with
no attachments, no explicit scenario descriptions in Allure and no emitted
scenario/instance/exchange identities. Contract field descriptions/examples,
complete current/history evidence and richer flow/rule links are later tasks;
the source/status guard does not claim those gaps are solved.

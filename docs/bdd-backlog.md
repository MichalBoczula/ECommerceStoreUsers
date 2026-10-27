# Users BDD and design documentation backlog

This is the service-specific plan agreed after the Users audit. Complete one
scoped PR at a time. Keep .NET Minimal API, application services, MongoDB
aggregate repositories and Customer/Admin current/history transactions.
Favorites have no history. ProductsCatalog supplies useful evidence patterns,
not a replacement architecture.

`[x]` means the task's implementation and checks are complete; `[ ]` remains.
The final LiveDocs selection belongs to task 12. Azure rollout is manual and
separate. Existing producer CI still creates report bundles and archives them
after successful master image publication; it does not deploy ACA.

- [x] **USERS-BDD-01 — Inventory and coverage guard.** Record every original
  scenario/example in [the baseline](bdd-01-baseline.md). Parse complete
  endpoint registrations, reject duplicate names/empty status sets, check
  source/matrix/OpenAPI operation and status coverage in both directions, and
  reject stale feature/scenario/outline-example links. Add focused regressions
  to the existing contract test script.
- [x] **USERS-BDD-02 — Shared HTTP evidence and identities.** Centralize all
  scenario clients, including fault and race clients. Add stable scenario,
  example-instance and exchange IDs, readable scenario descriptions, paired
  method/path/query/status/body evidence and JSON/malformed/text/204 formats.
  Keep the intentional OpenAPI bootstrap/export behavior. See
  [the evidence contract and verification scope](bdd-02-http-evidence.md).
- [x] **USERS-BDD-03 — Customer success scenarios.** Create/get/update
  individual/add company/update company: readable descriptions and Gherkin
  tables, full request/response evidence, explicit response expectations and
  persisted current/history/version assertions. See
  [Customer coverage and timestamp behavior](bdd-03-customers.md).
- [x] **USERS-BDD-04 — Admin success scenarios.** Create/get/update with full
  response and current/history/version assertions. An unchanged update is a
  successful no-op without a new version/history entry. See
  [Admin coverage and repeat-after-change evidence](bdd-04-admins.md).
- [x] **USERS-BDD-05 — Favorites success scenarios.** Add/list/remove/clear,
  exact collection contents, empty results, client isolation and clear no-op.
  Preserve the current ordering contract without promising a tie-breaker the
  repository does not implement. Do not add favorites history. See
  [complete collection and client-isolation evidence](bdd-05-favorites.md).
- [x] **USERS-BDD-06 — Business validation.** Cover active Customer/company/
  address/Admin/favorite rules and error signatures using isolated inputs,
  complete safe problems and unchanged stored data. Document actual behavior;
  do not activate unused rules as documentation cleanup. See
  [business rule and preservation evidence](bdd-06-business-validation.md).
  The [required-object follow-up](bdd-06-null-object-fix.md) corrects the three
  discovered null-object 500 cases to explicit 400 validation errors.
- [x] **USERS-BDD-07 — Framework failures.** Malformed JSON, missing/null
  members, wrong types/media, route/method/binding failures: exact request
  bytes, complete safe response and no writes. Distinguish framework rejection
  from business-policy validation. See
  [complete framework problems and persistence evidence](bdd-07-framework.md).
- [x] **USERS-BDD-08 — Concurrency and MongoDB atomicity.** Competing writes,
  unique-key races and full winner/current/history state. Distinguish mocked
  pre-repository failures from real MongoDB transaction rollback; retain the
  existing real history-insert rollback tests. Assess company add/update race
  causes independently from duplicate-tax-ID conflicts. See
  [full winning-state and real rollback evidence](bdd-08-atomicity.md), and the
  [creation-conflict fix](bdd-08-create-conflict-fix.md) for both Customer/Admin
  create paths.
- [x] **USERS-BDD-09 — Technical scenarios.** Health/readiness/startup,
  cancellation, isolation and documentation endpoints with truthful text,
  startup and database evidence. Keep appropriate lifecycle/export xUnit facts
  outside BDD and report their counts separately. See
  [technical outcomes, evidence and runner compatibility](bdd-09-technical.md).
- [x] **USERS-BDD-10 — Request/response contracts.** Validate input/output
  scalar types, required fields, nullability, formats, arrays and nesting;
  improve field descriptions and typed examples. Derive OpenAPI from source,
  retain real serializer behavior and avoid a second handwritten spec. See
  [generated schemas, typed examples and actual exchange checks](bdd-10-contracts.md).
- [ ] **USERS-BDD-11 — Flows, rules and RAG links.** Describe conditions,
  effects and failures for the 12 flows/92 steps; link operation → flow →
  rule/branch → scenario → exchange using stable identities. Classify enforced
  versus unused rules and reconcile stale engineering/publication guidance.
- [ ] **USERS-BDD-12 — Final audit and LiveDocs refresh.** Run full CI, inspect
  every expanded case and attachment, verify manifest hashes/sizes and rendered
  reports, select the successful master archive and publish the refreshed
  LiveDocs image. Record immutable producer/image provenance and the separate
  manual Azure rollout handoff.

Each PR uses the existing template and records actual commands, CI and report
evidence, checks that could not run, and remaining work. Preserve the original
cases while adding coverage; changes to their identities need an explicit
reviewed migration rather than silently replacing the baseline. Acceptance
counts and line coverage do not establish complete business-rule coverage.

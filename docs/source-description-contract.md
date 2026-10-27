# Users source descriptions and RAG evidence contract

USERS-BDD-11 keeps Minimal APIs, application services and MongoDB current/history
repositories. It adds source meaning and evidence joins. It does not implement
an agent, chunker, vector store, retrieval endpoint or retrieval evaluation.
There is no second handwritten OpenAPI or committed generated catalogue.

## Authoritative sources

| Knowledge | Source | Produced evidence |
| --- | --- | --- |
| Routes, DTO fields, media/status, nullable/required members and typed examples | API/DTO/serializer metadata | `sources/openapi.json` and actual contract observations |
| Nominal flow and ordered step meaning | The descriptor called by the application service, its Documentation/FlowStep attributes | Flow endpoint and generated operation-links catalogue |
| Outcome condition, effects, status and allowed problem codes | Class-owned FlowOutcome attributes | Structured branches in the same catalogue |
| Active policy/rule membership and complete errors | Domain registration and the rule collections used by Validate/Describe | Policy endpoint plus policy/rule IDs in the catalogue |
| Expected primary outcome and response error rules | Existing acceptance matrix with branchId/ruleIds | Scenario-to-branch/rule links |
| Technical routing/binding/media intent | Existing Gherkin branch labels or framework outline Code values | Technical scenario links without changing example identities |
| Observed behavior and state | Shared HTTP capture, contract checks and explicit BDD state assertions | Exchange-linked Allure attachments and complete current/history snapshots |

The legacy root retains `service: ECommerceStoreUsers`.
`documentation.serviceId: users` is the canonical bundle/evidence join value.
The generator follows the actual endpoint receiver, executed service method,
constructed descriptor and policy parameters; it does not infer flows from
similar service names or copy Products' CQRS architecture.

## Stable identities

Documentation attributes give flows, policies and rules explicit IDs. Preserve
them when changing wording or class names. A step ID combines its flow ID and
its existing explicit BPMN ID; method name, prose and step order are not identity
inputs. All 12 flow names, 92 BPMN IDs and their orders remain. Runtime stepName
now contains authored meaning, including conditions, effects and relevant
failures, rather than only a method name. The generator retains both the method
name and the step's description/source path.

FlowOutcome IDs identify declared outcome groups. Their conditions/effects,
HTTP status, allowed problem codes, rule context and application/persistence/
framework/documentation/exception-handler stage are exported. All 64 groups
have explicit scenario intent. They are not a claim to enumerate every internal
if/else path, every combination of invalid values or complete branch coverage.
The concurrent-create group explicitly allows resource_conflict or
concurrency_conflict; the actual problem code remains in its exchange evidence.

Match a scenario by scenarioId and an expanded example by its existing stable
instanceId. No original behavioral Examples column is added/removed/changed.
Join an HTTP pair and all observations by exchangeId. A matrix-bound primary
must match operation/status during When and must have the declared problem code
and expected error-rule IDs. Exactly one primary is required. Setup, competing
requests and follow-up reads remain separate actual exchanges. The peer-host
POST in the two-host case is explicitly supporting; it still keeps its original
When phase, request bytes and response. A duplicated primary fails completion.

`declaredStepIds` and `declaredPolicyIds` describe the nominal source flow.
`expectedBranchId` is scenario intent, not runtime telemetry. Actual error names
supply `observedErrorRuleIds`; a status alone cannot establish a rule or branch.
Rules outside the nominal flow, including an uninstalled rule, fail evidence
validation. The safe response and persisted-state assertions remain necessary.
A cancelled request gets a transport-failure link without inventing a status,
response or observed branch; its recorder completion is awaited before flushing.

## Enforced and unused rules

All six registered policies are invoked by endpoint flows. The source inventory
contains 20 Domain rule classes: **19 installed**, and
**CustomerCompanyTaxIdValidationRule uninstalled**. The installed collection rule
CompanyDataTaxIdUniquenessValidationRule supplies the Customer-wide check.
Do not activate or advertise the unused alternative as enforced cleanup.

The shared RequiredRequestObject mapping guard has its own inline identity. It
is not a Domain policy. Its actual error name is linked for explicit null
individual/billing/shipping objects. UUID validation checks emptiness, while
Favorites do not verify Customer/catalog existence. GET-by-external-ID flows
validate a synthetic profile with valid non-key fields; the catalogue does not
promise that a read validates or repairs a stored full profile.

Customer/company duplicate-tax failures are translated to 409 with an empty
errors array. The uniqueness rule is **declared branch context**, not an observed
error entry. Coverage reports distinguish rules expected in actual rejection
errors from a rule mapped to a conflict without error entries. No rule or outcome
is inferred from response status alone. Source coverage has no unlinked active
rule or declared outcome group after adding the missing initial-Customer-lookup
case for UpdateCompany. That does not prove every invalid field combination.

## State and failure boundaries

Customer/Admin writes use optimistic versions and atomic current/history
transactions. The Admin unchanged-profile path creates neither a new version nor
history. Customer updates have no general no-op promise; company-only updates
retain Customer UpdatedAt. Favorites use individual writes with no history.
Initial missing lookup, removal before save, and Favorite delete-missed races
have distinct outcome intent.

A generic internal_error belongs to the exception handler; it does not establish
which nominal step executed or whether a transaction wrote. Keep the existing
pre-write injected failures, actual history-insert rollback, winning concurrent
state and cancellation NoWrite evidence distinct. Metadata does not change those
production paths or authorize business-write retries.

## Verification and retrieval use

The existing source checks reject incomplete prose, duplicate IDs, orphan or
wrong-flow outcomes, status drift, unrelated/uninstalled rules and stale scenario
references. Export verification compares all 92 runtime step descriptions and
active policy memberships with the generated source projection. Runtime helper
regressions cover supporting/primary roles, duplicate primaries, missing actual
rules, wrong problem codes, uninstalled rules and response-less cancellation.
Full CI and independent inspection of the actual bundle remain required.

For retrieval, retain the bundle repository/commit/run/digest with every item.
Resolve all IDs inside that same immutable bundle. Use OpenAPI for wire shapes,
flow/outcome descriptions for decisions/effects, policy descriptions for possible
error signatures, and scenario/exchange/state attachments for observed behavior.
Do not mix a currently selected old portal report with a new producer archive,
or turn declared flow steps into a runtime trace. The catalogue and attachments
prepare these joins; actual ingestion/retrieval and evaluation remain future work.
Publication also does not prove historical design-first approval; preserve any
review evidence separately rather than inferring it from green tests.

The producer already publishes scanned master images and archives candidate
bundles for the existing LiveDocs portal. Task 12 owns final rendered-report
validation and selecting/refreshing the Users snapshot. Azure deployment remains
a separate manual action. Only synthetic acceptance data belongs in reports;
exclude credentials, production payloads and raw internal exception messages.

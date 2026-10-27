# USERS-BDD-12: final source and report audit

Status: **BDD source, successful master archive and rendered report audit complete**,
2026-10-27. No remaining gap was found in the agreed BDD tasks 1–11. The user
retained manual LiveDocs refresh and deployment. A new LiveDocs manifest selection,
image publication and Azure rollout are not claimed by this record; the complete
task-12 delivery checkbox remains open for those actions.

The audited snapshot is frozen to the producer below. A later documentation-only
commit does not change that archive's identity or silently select another report.

## Audited producer and archive

| Evidence | Identity |
| --- | --- |
| Users source | `906eccb87cd33dafc0a5516c4c0d6dcd80832c9b`, merged [PR #144](https://github.com/MichalBoczula/ECommerceStoreUsers/pull/144) |
| Successful master CI | [40555941884, attempt 1](https://github.com/MichalBoczula/ECommerceStoreUsers/actions/runs/40555941884) |
| Generated at | `2026-10-27T18:04:14Z` |
| Bundle SHA-256 | `04cc0fa66f64c1b25d3ed5130902d7ed11e3f1388d9e7b214e2cc3cdb1f42997` |
| Bundle size | 8,647,454 bytes |
| Bundle Actions artifact | [12757137674](https://github.com/MichalBoczula/ECommerceStoreUsers/actions/runs/40555941884/artifacts/12757137674) |
| Archive receipt artifact | [12757612605](https://github.com/MichalBoczula/ECommerceStoreUsers/actions/runs/40555941884/artifacts/12757612605) |
| Published Users service image | `mb0101/ecommerce-store-users-api@sha256:fd5a3acbdc59f449acdb01257a7f2d4ea003c572100dd03d9825bd4823a50b17` |

The image above is the **Users API image**, not a LiveDocs deployment image.
Master CI passed its build, formatting, contracts, coverage, secret/dependency
security checks, container build/scan/publication and Blob archival. Dependency
Review is PR-only and passed on the task-11 PR; it is correctly skipped on this
master run. Both published Users image tags reported the same scanned digest.

The archive receipt's commit/run/timestamp/hash/size match the independently
downloaded master bundle. This is a master archive, not the PR merge-ref bundle.
The outer Actions ZIP hashes were verified before extracting the bundle and TRX.

| Suite | Passed, no failures/skips | Line coverage |
| --- | ---: | ---: |
| Domain | 123 | 82% |
| Application | 54 | 92.5% |
| Infrastructure | 101 | 91% |
| Acceptance | 346 | — |

Acceptance contains **279 BDD cases and 67 standalone cases**; the standalone
checks are not presented as Gherkin scenarios. Separate Domain/Application/
Infrastructure line-coverage floors remain 70%. Mongo integration ran with Docker
kernel `6.8.0-1070-azure`. Local .NET/Docker were unavailable; runtime evidence
comes from actual CI. The source check also passed locally without changing gates.

## Complete structure and evidence checklist

| Requirement | Verified result |
| --- | --- |
| Discovery and retention | 47 features, 123 declarations (14 outlines), 279 expanded cases, 278 distinct scenario IDs and 279 distinct instance IDs. All original 81 baseline cases remain protected by the source gate; all 278 task-10 identities/titles/When-status multisets survive task 11. |
| Classic scenario structure | Every declaration has readable intent and Given/When/Then, including inherited Background steps. Every actual result has the same source description, resolved steps and recorded example parameters. |
| Gherkin tables | All declarations have step tables or Examples. 263 expanded cases render inline Data Table attachments; 16 use Examples instead: two database-marker examples, six controlled write faults and eight framework-contract examples. Those are explicit outline inputs, not missing table evidence. |
| Complete request/response evidence | 1,306 paired HTTP exchanges and nine request/transport-failure pairs retain service/scenario/instance/exchange IDs, method, path, query, phase, media type, bytes and outcomes. Setup/supporting exchanges remain distinct. |
| Truthful body formats | 2,549 JSON, 56 absent-body, eight empty-body, six text and two malformed-JSON envelopes. GET/no-body requests, health text, 204 responses and malformed inputs are not disguised as JSON documents. Cancellation has no fabricated response. |
| Archive integrity | Every one of 9,824 manifest file entries matches its byte count and SHA-256; archive inventory is closed. All 47 feature files byte-match the reviewed source. |
| Attachment integrity | All 9,215 attachments resolve, with no orphans: 8,329 JSON, 538 CSV tables and 348 text/docstring attachments. Every JSON-labelled attachment parses; raw malformed request bytes remain faithfully represented inside their evidence envelope. |
| Contracts | Bidirectional source/matrix/OpenAPI checks cover 14 named operations, 54 declared operation/status pairs and 250 matrix links. All 23 schemas, 83 described/typed/example fields and 12 route parameters remain verified against actual exchanges. |
| Flows and outcomes | 12 flows, 92 meaningful steps, six registered policies and 64 declared outcome groups. Runtime step prose/order/BPMN IDs and policy memberships match source; policy JSON remains unchanged. |
| Evidence joins | Every actual HTTP exchange has one documentation observation. There are 263 primary links (250 matrix + 13 technical bindings), 1,043 supporting/technical response links and nine truthful transport-failure links. The sole explicitly supporting peer POST keeps its original When/200 evidence. |
| Rules | All active rejection-rule identities have examples. Actual errors expose 18 installed Domain rule IDs plus the inline required-object guard. The installed uniqueness rule is conflict context, not an error-array observation; the unused alternative stays uninstalled. |
| Customers and Admins | Independent complete response/current/latest-history comparisons passed for 14 Customer cases, six Admin cases and one additional prior Admin update. Previous history remains exact; version increments, unchanged Admin no-ops, timestamps and unaffected companies/individual values are retained. |
| Favorites | All 11 success cases verify complete contents, insertion/removal/clear/no-op, other-client preservation and descending timestamps without inventing a tie-breaker or history collection. |
| Business validation | 130 rejection cases preserve all five collections, including 112 populated version-one setups and 83 isolated-value cases. All 13 null-object cases return safe 400 validation errors. Three accepted boundary cases remain successful. |
| Framework failures | All 53 retained framework cases have complete safe problems and preserved state; 38 raw requests were matched directly to Gherkin bytes. Subsequent serializer and initial-missing-Customer cases retain their additional contract/state checks. |
| Concurrency and atomicity | 20 deterministic atomicity cases: nine preserved winners, five deletion races and six real history-insert rollbacks. Two additional concurrent HTTP create cases retain one 200 winner and one safe 409 loser with complete five-collection preservation. Controlled pre-write faults remain separate. |
| Technical behavior | All 27 technical cases retain health, safe faults, cancellation, documentation and isolation evidence. Standalone/TRX checks independently confirm seven lifecycle cases, readiness 200/503/200, five indexed collections and three real cancelled-driver repository cases. |

## Generated Allure report

Installed the repository-pinned Allure **2.46.1**, verifying its official
distribution SHA-256, then generated the actual master results. The generated
summary contains exactly 279 passed cases, zero failed/broken/skipped/unknown.

The existing report-case HTTP checker visited **every** generated case and
attachment through a local HTTP server. Additional comparisons matched each
rendered instance, title, description, ordered steps and every attachment's
name/type/bytes to its source result. All entry assets resolved; JSON attachments
parsed. The 538 rendered CSV attachments preserve actual Gherkin tables.
Outline titles retain the adapter's template names; example parameters, stable
instance identities and resolved steps distinguish their results.

This checks the generated report and its HTTP resources. It does not claim a
pixel-by-pixel browser review, a new Nginx production image, or a live Azure check.
The existing LiveDocs image smoke test should run when the manual refresh builds
that image.

## Recorded limits

- The agreed scenario inventory and every declared outcome group are linked;
  this does not prove every invalid-value combination or all internal branches.
- Flow/step metadata and scenario branch intent are declared knowledge, not
  executed step/branch telemetry. The uniqueness-to-409 mapping and uninstalled
  rule are classified explicitly rather than counted as observed rule errors.
- Pre-write cancellation/fault tests prove no writes, while the six actual Mongo
  history-insert failures independently prove transaction rollback. Hosted Azure
  DocumentDB compatibility and production recovery are separate verification.
- Source-generated contracts and green tests do not establish historical
  design-first approval. RAG ingestion, retrieval and evaluation remain future
  work; the source and evidence joins needed for them are present.

## Manual LiveDocs refresh handoff

The inspected LiveDocs main commit is
`0ca91fc8be1231f6b6f8ad17c41226e3bc2bf6f2`. Its development-v1 manifest still
selects Users commit `112c4be06a79ef1f0653e1e2d8d6de0538e7d611`.
Redeploying an image built from that unchanged selection cannot show this audit's
new report. The host serves reports baked into its image; it does not fetch the
latest Blob contents at runtime.

For the manual refresh, replace **only the Users project reference** in
LiveDocs `manifests/portal.json` with the archived receipt below. Preserve the
other projects, version state and latest pointer.

```json
{
  "project": "users",
  "commitSha": "906eccb87cd33dafc0a5516c4c0d6dcd80832c9b",
  "generatedAt": "2026-10-27T18:04:14Z",
  "workflowRunId": 40555941884,
  "artifact": {
    "url": "https://stecproductsdevmb261013.blob.core.windows.net/livedocs/reports/users/906eccb87cd33dafc0a5516c4c0d6dcd80832c9b/04cc0fa66f64c1b25d3ed5130902d7ed11e3f1388d9e7b214e2cc3cdb1f42997.zip",
    "sha256": "04cc0fa66f64c1b25d3ed5130902d7ed11e3f1388d9e7b214e2cc3cdb1f42997",
    "sizeBytes": 8647454
  }
}
```

Build/verify/publish the selected LiveDocs image through its normal CI and use
the resulting **LiveDocs** digest from `livedocs-image/image.json`. Update only
`app_images.livedocs` in the ignored shared Terraform variables, review a fresh
plan, then apply it manually. Do not use the Users service-image digest above.

After rollout, compare `/build-info.json` with the newly published LiveDocs image
metadata, and verify `/livedoc/v1/users/bundle.json` has the audited Users commit,
run and timestamp. The Allure summary at
`/livedoc/v1/users/allure/widgets/summary.json` must report 279 passed cases.
Open `/livedoc/v1/users/allure/` for the browser review. Record the new LiveDocs
commit, CI run and digest before marking full task-12 delivery complete.

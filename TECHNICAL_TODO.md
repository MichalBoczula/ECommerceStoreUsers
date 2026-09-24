# ECommerceStoreUsers technical backlog

This file reflects the code and CI after REF-11 and ADR review REF-12/4.
`[x]` means implemented and checked, `[ ]` remains, and `[>]` is deferred
beyond this reference-service development phase. Green CI does not approve a
public production deployment. ProductsCatalog is an equal reference service,
not an architecture template for MongoDB application services.

## Implemented and checked

- [x] Safe problem responses with stable codes and generic public 500;
  health endpoints retain documented plain text.
- [x] OpenAPI exported and linted in CI; acceptance HTTP status, media type
  and DTO shape compared to the generated document for exercised scenarios.
- [x] Named operations linked from endpoints to executed service flows,
  registered policies and Reqnroll scenario IDs. Missing/duplicate links fail.
- [x] Status/cause acceptance matrix includes controlled write failures,
  rollback, duplicate requests, concurrency and deterministic reads.
- [x] One MongoDB replica-set container per acceptance run; separate database,
  API host and client per scenario, with cleanup even on failure.
- [x] Domain and Application 70% line-coverage gates, Infrastructure coverage
  for diagnosis, all test layers in CI, TRX/report artifacts and format gate.
- [x] Architecture gate for layer/persistence boundaries with negative tests.
- [x] MongoDB configuration fail-fast; named indexes safe on repeated/concurrent
  startup. Conflicting definitions stop startup rather than being dropped.
- [x] `/health/live`, `/health/ready` and compatibility `/health`; readiness
  checks a writable replica-set primary and sessions.
- [x] Bounded read-only startup probe, cancellation and host-stop behavior;
  no blanket retry of business transactions or index creation.
- [x] Customer/Admin `Version` matching and atomic current/history writes;
  Admin no-op avoids new history. Favorites use a unique client/product index.
  Repository tests cover outcomes, conflicts and rollback.
- [x] Local README, health/startup/index guidance and ADRs for MongoDB,
  errors, test isolation, CI and generated documentation.
- [x] NuGet high/critical audit, Dependency Review on PRs, Gitleaks, Trivy,
  and Docker build after the quality gate. Ordinary compiler warnings remain
  visible. No Users image is published by this repository today.

## Open for a production environment

- [ ] Implement authentication and policy authorization for actual consumers,
  including scopes/roles and 401/403 scenarios. Current endpoints are not safe
  for public exposure without access control.
- [ ] Define production secret delivery, network exposure, personal-data
  masking and logging policy. Local `.env` is not a production design.
- [ ] Define backup/restore, history retention and controlled upgrades on a
  populated MongoDB database. [Index evolution](docs/mongo-index-evolution.md)
  is documented; no automatic schemaVersion or general migration framework exists.
- [ ] Define service signals, tracing, metrics, dashboards and alerts.
  Error `traceId` does not imply distributed trace propagation in all logs.
- [ ] Decide registry, deployment, rollback and post-deployment smoke tests.
  Verify required checks/rulesets and native GitHub secret-scanning settings
  with administrative access; workflow presence does not prove those settings.

## Deferred or conditional

- [>] Hosted Allure/API reports, LiveDocs and RAG ingestion/retrieval. Current
  generated sources and TRX/coverage artifacts are not a hosted portal.
- [>] Kiota compatibility belongs to consumers generating a client, such as
  Invoice/BFF. This producer stores no generated clients.
- [>] API versioning, browser CORS, rate limits, pagination, larger request
  limits and generic idempotency keys depend on real consumer and load needs.
  JSON contract inspection already has a documented 64 KiB bound.
- [>] Scheduled benchmarks, performance budgets, dependency automation and
  reusable CI workflows follow portfolio-wide evaluation. Keep current gates.
- [>] Broader write retry policies require operation-specific idempotency;
  a bounded connection probe does not authorize transaction retries.

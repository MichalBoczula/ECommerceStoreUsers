# Technical TODO

This backlog tracks cross-cutting and production-readiness work that should be implemented after the complete application flow is in place.

## P0 - Required before production

### Security

- [ ] Add authentication using OpenID Connect and JWT bearer tokens.
- [ ] Add policy-based authorization for customer and administrator operations.
- [ ] Define the required scopes, application roles, and authorization failure contract.
- [x] Move development credentials out of committed configuration.
- [ ] Add secret scanning and dependency vulnerability scanning to CI.
- [ ] Define rules for masking personal data and secrets in logs.

### Configuration and startup

- [x] Validate all MongoDB application settings during startup and fail fast on invalid configuration.
- [ ] Add environment-specific configuration documentation.
- [ ] Make MongoDB index initialization safe for concurrent application startup.
- [ ] Define an explicit strategy for MongoDB index and document-schema evolution.

### Health and reliability

- [ ] Split health endpoints into liveness and readiness checks.
- [ ] Add MongoDB connectivity and write-readiness checks.
- [ ] Add request timeout and cancellation handling.
- [ ] Define retry rules for transient MongoDB errors and transaction retries.
- [ ] Add graceful shutdown validation.

### Observability

- [ ] Add structured JSON logging.
- [ ] Add correlation and trace identifiers to requests, logs, and error responses.
- [ ] Add OpenTelemetry tracing and metrics.
- [ ] Define service-level indicators for latency, errors, throughput, and dependency failures.
- [ ] Add dashboards and production alerts.

## P1 - Quality and delivery

### API platform

- [ ] Define an API versioning strategy.
- [ ] Configure CORS for known consumers.
- [ ] Add rate limiting where required.
- [ ] Define idempotency requirements for write operations.
- [ ] Add pagination standards for collection endpoints.
- [ ] Add request-size limits.
- [ ] Export and validate the OpenAPI document in CI.
- [ ] Add generated-client compatibility checks for Kiota consumers.

### Testing

- [x] Generate code coverage reports in CI.
- [x] Introduce minimum coverage thresholds for Domain and Application.
- [x] Run Infrastructure integration tests in CI.
- [ ] Optimize acceptance tests to reuse a MongoDB container while isolating scenario data.
- [ ] Add architecture tests that enforce layer dependencies.
- [ ] Add API contract tests for generated clients.
- [ ] Add concurrency and duplicate-request integration tests.
- [ ] Move benchmarks to a dedicated manually triggered or scheduled workflow.
- [ ] Store Allure and test-result artifacts from CI.

### CI/CD

- [x] Add formatting and static-analysis checks.
- [ ] Add Docker image build validation.
- [ ] Add container image vulnerability scanning.
- [ ] Add a deployment workflow with environment approvals.
- [ ] Add an automated smoke test after deployment.
- [ ] Add dependency update automation.
- [ ] Protect the default branch with required checks.
- [ ] Remove duplicated restore/setup logic from the workflow when the pipeline is stabilized.

## P1 - Data consistency

- [ ] Add optimistic concurrency control for aggregate updates.
- [ ] Verify the result of replace, update, and delete operations.
- [ ] Define retention rules for history collections.
- [ ] Define backup and restore procedures.
- [ ] Document transaction boundaries and consistency guarantees.
- [ ] Add cleanup and retention rules for test and non-production databases.

## P2 - Maintainability

- [ ] Add Architecture Decision Records for the layer structure, MongoDB, history model, and generated API clients.
- [ ] Add local-development and troubleshooting documentation.
- [ ] Add a dependency review process and remove unused packages regularly.
- [ ] Standardize error codes independently from human-readable messages.
- [ ] Add performance budgets and regression thresholds.
- [ ] Decide whether flow and validation descriptors remain custom endpoints or become part of generated documentation.
- [ ] Add a production-readiness checklist for new endpoints.

## Completed maintenance

- [x] Remove unused SQL Server dependency from Acceptance Tests.
- [x] Remove unused Playwright dependency from Acceptance Tests.

# ADR-0004: Gate quality and scan a local image

- Status: Accepted
- Date: 2026-09-24

## Context

The service needs repeatable quality checks without making every ordinary
compiler warning a release blocker. Tests and security findings have distinct
failure policies. A built image should be scanned after the source checks.

## Decision

CI verifies source links, operation-to-flow/policy/scenario relationships and
architecture boundaries, then restores, checks format, builds and validates
generated OpenAPI with a pinned Redocly CLI. Domain, Application,
Infrastructure and HTTP acceptance suites run separately with TRX and coverage
reports. Domain and Application each require at least 70% line coverage;
Infrastructure reports coverage without a percentage threshold.

Enable NuGet audit for high/critical findings (`NU1903`/`NU1904` as errors),
Dependency Review on pull requests, Gitleaks, and a Trivy high/critical scan
of the local Docker image after the quality gate. Compiler warnings remain
visible without global warnings-as-errors. CI builds but does not publish a
Users image. The exact jobs and event conditions are in
[`ci.yml`](../../.github/workflows/ci.yml).

## Consequences

Build, tests, coverage, contracts and agreed security findings can block image
work. A coverage percentage does not replace the acceptance status/cause
matrix. A green image scan verifies the locally built image only: registry
publication, provenance, deployment and rollout remain separate decisions.

## Alternatives considered

- Treat all warnings as errors: conflates ordinary diagnostics with explicitly
  selected quality and vulnerability gates.
- Publish an image from every PR: would distribute changes before merge and
  without an agreed destination registry for Users.

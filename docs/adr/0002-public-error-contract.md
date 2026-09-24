# ADR-0002: Return safe, stable problem responses

- Status: Accepted
- Date: 2026-09-24

## Context

Users endpoints can fail during binding, domain validation, missing-resource
checks, business conflicts, MongoDB writes or route matching. Clients need a
consistent contract without receiving internal exception details.

## Decision

Translate controlled failures into `application/problem+json` with HTTP
`status`, a stable `code`, safe detail, request path and trace ID. Domain
validation may include structured errors; missing JSON properties are reported
only when inferred from the request contract and a valid bounded body.
Unexpected failures expose a generic 500 and keep details in server logs.
Routing errors, unsupported content types and known resource conflicts also
use the safe response. Health endpoints retain their framework plain-text
responses, including unhealthy readiness.

Generated OpenAPI and acceptance HTTP checks guard declared status, media type
and response schema. The current code list and technical exceptions live in
[the error contract](../api-problem-contract.md), rather than a copied table in
this ADR.

## Consequences

Consumers can branch on `code`, not text. A new failure branch requires
endpoint metadata and acceptance coverage. An error produced before a matched
operation executes may need a separate framework-level acceptance assertion.
Server diagnostics are required to investigate a sanitized 500.

## Alternatives considered

- Raw framework or database errors: expose unstable internals.
- Independent handwritten HTTP error specification: can drift from runtime and
  generated OpenAPI.

# ADR-0005: Derive API and flow documentation from executable sources

- Status: Accepted
- Date: 2026-09-24

## Context

Hand-maintained route, flow and policy catalogs drift from an evolving API.
Users has application services and MongoDB rather than the CQRS/MediatR model
used by ProductsCatalog, so its links must follow its own execution structure.

## Decision

Generate OpenAPI from endpoint metadata and DTOs. Keep ordered flow steps next
to the descriptors called by application services, and expose flow and
validation-policy descriptions under `/users-documentation/*`. Link named HTTP
operations to executed service methods, descriptors, registered policies and
Reqnroll scenario IDs by inspecting source and the existing acceptance matrix.
CI checks missing/duplicate links, validates generated OpenAPI, and compares
exercised HTTP responses with the document. Generated projections are build
artifacts, not committed parallel specifications or handwritten JSON examples.
Keep [`ADR-0001`](0001-mongodb-documents-and-history.md) and other decision
records as authored explanations of intent; they do not duplicate per-operation
generated data.

## Consequences

Changing routes, validation or flows can break CI until the executable sources
and acceptance scenarios agree. The runtime comparison establishes what the
acceptance suite exercises, not every conceivable response. The current CI
does not host a documentation portal, publish Allure reports, ingest documents
into a RAG store or evaluate retrieval answers. Those are future proposals,
not a feature accepted as implemented by this ADR.

## Alternatives considered

- A separately maintained OpenAPI and operation/flow table: creates extra
  synchronization and opportunities for drift.
- Copy Products' CQRS linking model into Users: changes service architecture
  only to match a tool, with no business reason.

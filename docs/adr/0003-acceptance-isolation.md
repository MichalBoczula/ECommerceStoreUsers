# ADR-0003: Isolate acceptance scenarios on one MongoDB replica set

- Status: Accepted
- Date: 2026-09-24

## Context

HTTP acceptance tests must cover real MongoDB transactions, indexes, current
documents and history. Sharing one database among scenarios would couple their
state, while starting a new container for each would multiply setup time.

## Decision

Use one MongoDB replica-set Testcontainer per acceptance test run. Give each
Reqnroll scenario a distinct database, application factory and HTTP client.
Dispose the host and client and drop its database after every scenario, even
when setup or execution fails. Read the generated OpenAPI from the test host;
the response handler checks actual HTTP outcomes against declared status,
media type and response schema. `.feature` files and step definitions are the
source; generated `.feature.cs` files are not committed.

Infrastructure tests use MongoDB Testcontainers for repository, mapping,
transaction and index behavior. The separate OpenAPI export test can start
without a running MongoDB instance. See [local verification](../local-verification.md).

## Consequences

The expensive MongoDB process is reused while scenarios cannot share current
or history documents. Cleanup failures must be visible. This verifies isolated
test data, not an upgrade of a production database or long-term history
retention. A live Docker daemon is required for the MongoDB suites.

## Alternatives considered

- Shared acceptance database: makes scenarios order dependent.
- A replica-set container per scenario: isolates data but adds setup cost
  without improving the database boundary.

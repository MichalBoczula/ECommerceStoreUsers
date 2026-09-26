# Architecture Decision Records

Records describe decisions implemented in this service. A later decision may
supersede a record without erasing the original context.

Use repository-local, sequential ADR numbers. Each record has a title, `Status` and `Date` metadata, then `Context`, `Decision`, `Consequences` and `Alternatives considered` sections in that order. Keep accepted records when later decisions supersede them, and add the new record to this index.

| ADR | Status | Decision |
| --- | --- | --- |
| [0001](0001-mongodb-documents-and-history.md) | Accepted | MongoDB documents, current/history writes, transactions, indexes and schema evolution. |
| [0002](0002-public-error-contract.md) | Accepted | Return safe problem responses with stable codes. |
| [0003](0003-acceptance-isolation.md) | Accepted | Isolate scenarios on one MongoDB replica set. |
| [0004](0004-ci-and-security-gates.md) | Accepted | Gate quality and scan a local, unpublished image. |
| [0005](0005-generated-documentation-sources.md) | Accepted | Generate OpenAPI and flow links from executed sources. |
